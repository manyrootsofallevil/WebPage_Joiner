using HostFeed;
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
    //So it seems that the simplest way of getting periodicals is to create a RSS feed of all the links and let
    //Calibre do its work. However, this negates all the work done here, which is a bit of a bastard but there you go
    //Also there is no control whatsoever over the conversion process, so Calibre has issues then that's that.
    //I guess an alternative is to offer both options, i.e. periodicals in which case only links will be needed and full
    //processing, in which case the files will be needed, but this raises the question of why do we need the files, 
    //just download them and be done with it.
    //I guess the files could be hosted after they have been stripped of all the stuff but it seems awfully convoluted.


    //1. Convert Saved Web Pages to Mobi.
    //   Simplest way is to create an RSS feed of the saved pages addresses and let Calibre sort it out.
    //2. Create Chrome Extension that saves urls in file [wouldn't be simpler to save them as bookmarks?] 
    //so that thye can be feed to 1.
    // 3. Convert individual files to mobi, is this needed?

    public enum AppType { Browser, EbookConverter };

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static XDocument domains;
        private CollectionViewSource fileList = new CollectionViewSource();

        private readonly BackgroundWorker feed = new BackgroundWorker();



        public MainWindow()
        {
            this.ItemList = new ObservableCollection<HTMLPage>();
            fileList.Source = ItemList;
            //this.fileList.SortDescriptions.Add(new SortDescription("HTMLPage.FileName",
            //   ListSortDirection.Ascending));

            InitializeComponent();
            DataContext = this;

            feed.DoWork += (o, e) => { Host.Start(); };
            feed.RunWorkerCompleted += (o, e) => { //run ebook converter here. Give full path to recipe
            };
        }

        public CollectionViewSource FileList
        {
            get { return fileList; }

        }

        public ObservableCollection<HTMLPage> ItemList
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
                    this.ItemList.Add(new HTMLPage(st));
                }
            }
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {

            //if (Items.SelectedItems.Count > 0)
            //{
            SaveFileDialog save = InstatiateSaveDialog(ItemList.Count() > 1 ? true : false);

            if ((bool)save.ShowDialog())
            {
                try
                {
                    HtmlDocument doc = new HtmlDocument();
                    StreamWriter sr = new StreamWriter(@"C:\urls.txt");

                    //foreach (string file in Items.SelectedItems)
                    foreach (HTMLPage page in FileList.View)
                    {

                        HtmlDocument docx = new HtmlDocument();
                        
                        docx.Load(page.GetPage(), System.Text.Encoding.UTF8, false);


                        var domain = docx.DocumentNode.ChildNodes
.Where(x => x.Name.Contains("comment") && x.InnerHtml.Contains("saved from"))
.FirstOrDefault().InnerHtml;

                        string url = domain.Split(')')[1].Remove(domain.Split(')')[1].Length - 3).Trim();
                        sr.WriteLine(url);
                        //SaveHTMLToFile(save, doc, page.GetPage());
                    }
                    sr.Flush();
                    sr.Close();
                    if ((bool)Convert.IsChecked)
                    {

                        string saveFile = string.Format(@"{0}\{1}", System.IO.Path.GetDirectoryName(save.FileName),
                            System.IO.Path.GetFileNameWithoutExtension(save.FileName));
                        //TODO: Add Author, etc...
                        //TODO: http://manual.calibre-ebook.com/conversion.html

                        //ADD <H1> tag with article title before each new page.

                        RunExternalApplication(AppType.EbookConverter,
                            string.Format("{0} {1}.mobi --authors {2}", save.FileName, saveFile, "YestMen"));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Error: {0} - {1}.", ex, ex.GetType()));
                }
            }
            //}
            //else
            //{
            //    MessageBox.Show("Select at least one item");
            //}
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
                    //ItemList.Remove();
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

            doc.Load(file.ToString(), System.Text.Encoding.UTF8, false);

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
            if ((bool)Learn.IsChecked)
            {
                RunExternalApplication(AppType.Browser, string.Format("\"{0}\"", save.FileName));
            }


            if ((bool)!Learn.IsChecked || MessageBoxResult.Yes == MessageBox.Show("Are you Satisfied with the Conversion", "OK", MessageBoxButton.YesNo))
            {
                if ((bool)Delete.IsChecked)
                {
                    File.Delete(file);
                }

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

        private SaveFileDialog InstatiateSaveDialog(bool multiple)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.AddExtension = true;
            save.CheckPathExists = true;
            save.Filter = "HTML Files|*.htm;*.html";
            if (multiple)
            {
                save.FileName = string.Format("{0}.html", DateTime.Now.ToString("yyyyMMdd"));
            }
            else
            {
                save.FileName = "article.html";
            }
            return save;
        }

        public static void AddDomainToFile(string Id, string domain)
        {
            try
            {
                XElement site = new XElement("domain",
             new XAttribute("name", domain),
             new XAttribute("content", Id));

                domains.Root.Add(site);

                domains.Save(ConfigurationManager.AppSettings["path"]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public static void AddDomainToFile(string domain, string tag, string type, string content)
        {
            try
            {
                XElement site = new XElement("domain",
            new XAttribute("name", domain),
            new XAttribute("tag", tag),
            new XAttribute("type", type),
            new XAttribute("content", content));

                domains.Root.Add(site);

                domains.Save(ConfigurationManager.AppSettings["path"]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
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
            try
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
                    //TODO: What the hell is this?????
                    //answer. Encoding issues.
                    //sw.Write(content.InnerHtml.Replace("â€™", "'").Replace("â€œ", "\"").Replace("â€”", "—").Replace("â€", "\""));
                    sw.Write(content.InnerHtml);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
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

        private void Periodical_Click(object sender, RoutedEventArgs e)
        {
            Convert.IsChecked = true;
            feed.RunWorkerAsync();
            RunExternalApplication(AppType.EbookConverter,
                 string.Format("Custom.recipe {0}.mobi --authors {1}", @"c:\saveFile", "YesMen"));
        }



        //TODO: Delete directoires as well as files if appropriate
    }
}
