using HtmlAgilityPack;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Xml.Linq;

namespace HTMLJoiner
{

    public class TagData
    {
        string name, tag, type, content;

        public TagData() { }

        public TagData(string name, string tag, string type, string content)
        {
            this.name = name;
            this.tag = tag;
            this.type = type;
            this.content = content;
        }


        public string Name { get; set; }
        public string Tag { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
    }

    public enum AppType { Browser, EbookConverter };

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static XDocument domains;

        public MainWindow()
        {
            this.ItemList = new ObservableCollection<string>();
            InitializeComponent();
            DataContext = this;
        }

        public ObservableCollection<string> ItemList
        {
            get;
            private set;
        }

        private void LoadFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog files = new OpenFileDialog();
            files.AddExtension = true;
            files.CheckFileExists = true;
            files.Multiselect = true;
            files.Filter = "HTML Files|*.htm;*.html";
            files.FilterIndex = 1;

            if ((bool)files.ShowDialog())
            {
                foreach (string st in files.FileNames)
                {
                    this.ItemList.Add(st);
                }
            }
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {

            if (Items.SelectedItems.Count > 0)
            {
                SaveFileDialog save = InstatiateSaveDialog();

                if ((bool)save.ShowDialog())
                {
                    try
                    {
                        HtmlDocument doc = new HtmlDocument();

                        foreach (string file in Items.SelectedItems)
                        {

                            SaveHTMLToFile(save, doc, file);

                        }

                        if ((bool)Convert.IsChecked)
                        {

                            string saveFile = string.Format(@"{0}\{1}", System.IO.Path.GetDirectoryName(save.FileName),
                                System.IO.Path.GetFileNameWithoutExtension(save.FileName));


                            RunExternalApplication(AppType.EbookConverter,
                                string.Format("{0} {1}.mobi", save.FileName, saveFile));
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("Error: {0} - {1}.", ex.Message, ex.GetType()));
                    }
                }
            }
            else
            {
                MessageBox.Show("Select at least one item");
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ItemList.Clear();
        }

        private void ClearSelected_Click(object sender, RoutedEventArgs e)
        {
            if (Items.SelectedItems.Count > 0)
            {
                for (int i = 0; i < Items.SelectedItems.Count; i++)
                {
                    ItemList.Remove(Items.SelectedItems[i].ToString());
                }
            }

        }



        private void SaveHTMLToFile(SaveFileDialog save, HtmlDocument doc, string file)
        {
            //Allthough this is inefficient, it ensures that it will process recently added domains correctly
            //TODO: Look for a better way
            domains = LoadDomains();

            HtmlNode content = null;
            HtmlAttribute attribute = null;

            doc.Load(file.ToString());

            string domain = GetDomain(doc);
            TagData tag = GetContentTag(domains, domain);

            //If the tag is not empty then we know what we are searching for so we use it
            if (!string.IsNullOrEmpty(tag.Content) && string.IsNullOrEmpty(tag.Tag))
            {
                content = doc.DocumentNode.Descendants()
                   .Where(x => x.Id == tag.Content).FirstOrDefault();
            }
            //Clearly this is sub optimal, but there you go for the time being.
            //TODO:Improve.
            else if (!string.IsNullOrEmpty(tag.Content) && !string.IsNullOrEmpty(tag.Tag))
            {
                var tags = doc.DocumentNode.Descendants()
                    .Where(x => x.Name == tag.Tag);

                foreach (var item in tags)
                {
                    var relevantTag = item.Attributes.Where(x => x.Name == tag.Type && x.Value == tag.Content).FirstOrDefault();

                    if (relevantTag != null)
                    {
                        attribute = relevantTag;
                        content = item;
                        break;
                    }
                }
            }
            else
            {
                //Grab content tag. Clearly this is unlikely to work for everything.
                content = doc.DocumentNode.Descendants()
                    .Where(x => x.Id.Contains("main") || x.Id.Contains("article")
                        || x.Id.Contains("content") || x.Id.Contains("container")).FirstOrDefault();

            }


            if (content != null)
            {
                //Remove all the nasties and Save to File
                PurifyAndSave(save.FileName, content, doc.Encoding);
            }
            //Fail safe. Not sure I like it
            else
            {
                //Remove all the nasties and Save to File
                PurifyAndSave(save.FileName, doc.DocumentNode, doc.Encoding);
            }


            //Starting Browser to check how well the page is being displayed
            RunExternalApplication(AppType.Browser, string.Format("\"{0}\"", save.FileName));


            if ((bool)!Learn.IsChecked || MessageBoxResult.Yes == MessageBox.Show("Are you Satisfied with the Conversion", "OK", MessageBoxButton.YesNo))
            {
                if (!string.IsNullOrEmpty(content.Id) && domains.Root.Descendants()
                        .Where(x => x.Attribute("name").Value == domain).FirstOrDefault() == null)
                {

                    AddDomainToFile(content.Id, domain);

                }
                //Is this actually going to ever be used??
                else if (domains.Root.Descendants()
                        .Where(x => x.Attribute("name").Value == domain).FirstOrDefault() == null)
                {
                    AddDomainToFile(domain, content.Name, attribute.Name, attribute.Value);
                }
            }
            //If unhappy, use F12 to select the correct part.
            //TODO: allow this to be changed to another browser
            else
            {
                //Delete File as we did not like the output
                File.Delete(save.FileName);
                //Run Browser with original file
                RunExternalApplication(AppType.Browser, string.Format("\"{0}\"", file));
                //Run the Improve Me Dialog.
                ImproveMe cc = new ImproveMe(domain);

                if ((bool)cc.ShowDialog())
                {
                    //Try Again;
                    SaveHTMLToFile(save, doc, file);
                }
            }
        }

        private void RunExternalApplication(AppType app, string arguments)
        {
            switch (app)
            {
                case AppType.Browser:
                    Process.Start(ConfigurationManager.AppSettings["Browser"], arguments);
                    break;
                case AppType.EbookConverter:
                    Process.Start(ConfigurationManager.AppSettings["EbookConverter"], arguments);
                    break;


            }
            //TODO: Have a look at this to get the ouput from ebook converter process
            //            var proc = new Process {
            //    StartInfo = new ProcessStartInfo {
            //        FileName = "program.exe",
            //        Arguments = "command line arguments to your executable",
            //        UseShellExecute = false,
            //        RedirectStandardOutput = true,
            //        CreateNoWindow = true
            //    }
            //};
            //then start the process and read from it:

            //proc.Start();
            //while (!proc.StandardOutput.EndOfStream)
            //{
            //    string line = proc.StandardOutput.ReadLine();
            //    // do something with line
            //}
        }

        private SaveFileDialog InstatiateSaveDialog()
        {
            SaveFileDialog save = new SaveFileDialog();
            save.AddExtension = true;
            save.CheckPathExists = true;
            save.Filter = "HTML Files|*.htm;*.html";
            save.FileName = System.IO.Path.GetFileName(Items.SelectedItems[0].ToString());
            return save;
        }

        public static void AddDomainToFile(string Id, string domain)
        {
            XElement site = new XElement("domain",
                new XAttribute("name", domain),
                new XAttribute("content", Id));

            domains.Root.Add(site);

            domains.Save(ConfigurationManager.AppSettings["path"]);
        }

        public static void AddDomainToFile(string domain, string tag, string type, string content)
        {
            XElement site = new XElement("domain",
                new XAttribute("name", domain),
                new XAttribute("tag", tag),
                new XAttribute("type", type),
                new XAttribute("content", content));

            domains.Root.Add(site);

            domains.Save(ConfigurationManager.AppSettings["path"]);
        }

        /// <summary>
        /// Grab the domain from which the web page was saved.
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private static string GetDomain(HtmlDocument doc)
        {
            var domain = doc.DocumentNode.ChildNodes
                .Where(x => x.Name.Contains("comment") && x.InnerHtml.Contains("saved from"))
                .FirstOrDefault().InnerHtml;

            domain = Regex.Match(domain, @"([^:]*:\/\/)?([^\/]+\.[^\/]+)").Groups[2].Value;

            return domain;
        }

        /// <summary>
        /// Get the HTML tag that has the content, i.e. the meat of the article.
        /// </summary>
        /// <param name="domains">File containing all the domains that have been successfully processed</param>
        /// <param name="domain">Domain to check</param>
        /// <returns></returns>
        private static TagData GetContentTag(XDocument domains, string domain)
        {
            TagData result = new TagData();

            var content = domains.Root.Descendants()
                .Where(x => x.Attribute("name").Value == domain).FirstOrDefault();

            if (content != null)
            {
                result.Content = content.Attributes("content").FirstOrDefault().Value;
                result.Name = content.Attributes("name").FirstOrDefault().Value;
                result.Tag = content.Attributes("tag").FirstOrDefault() == null ?
                    string.Empty : content.Attributes("tag").FirstOrDefault().Value;
                result.Type = content.Attributes("type").FirstOrDefault() == null ?
                    string.Empty : content.Attributes("type").FirstOrDefault().Value;

            }

            return result;
        }

        /// <summary>
        /// Load the file containing the sucessfully processed web pages (by domain).
        /// This rather assumes CMS on the domain, which is probably a fair assumption.
        /// </summary>
        /// <returns></returns>
        private static XDocument LoadDomains()
        {
            return XDocument.Load(ConfigurationManager.AppSettings["path"]);
        }

        private static void PurifyAndSave(string fileName, HtmlNode content, Encoding encoding)
        {
            content.Descendants()
                .Where(x => x.Name == "script" || x.Name == "iframe" || x.Name == "noscript"
                    || x.Name == "form" || x.Name == "#comment" || x.Id.Contains("comment")).ToList()
                .ForEach(x => x.Remove());

            var tolo = content.Descendants();
            var tol =
            content.Descendants()
                .Where(x => x.Name == "script" || x.Name == "iframe" || x.Name == "noscript" || x.Name == "form" || x.Id.Contains("comment")).ToList();

            using (StreamWriter sw = new StreamWriter(fileName, true, encoding))
            {
                sw.Write(content.InnerHtml.Replace("â€™", "'").Replace("â€œ", "\"").Replace("â€", "\""));
            }
        }

        //NOT in USE
        private HtmlNode FindInner(HtmlNode node)
        {
            HtmlNode result = null;

            HtmlNode content = node.Descendants()
            .Where(x => x.Id.Contains("main") || x.Id.Contains("article") || x.Id.Contains("content") || x.Id.Contains("container")).FirstOrDefault();

            if (content != null)
            {
                result = FindInner(content);
            }
            else
            {
                result = content;
            }

            return result;
        }

        private static bool CheckDomainExists(XDocument domains, string domain)
        {
            return domains.Root.Descendants().Where(x => x.Name == domain).Count() == 1;
        }


    }
}
