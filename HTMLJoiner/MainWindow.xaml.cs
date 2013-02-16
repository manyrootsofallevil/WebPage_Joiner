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
                SaveFileDialog save = new SaveFileDialog();
                save.AddExtension = true;
                save.CheckPathExists = true;
                save.Filter = "HTML Files|*.htm;*.html";
                save.FileName = System.IO.Path.GetFileName(Items.SelectedItems[0].ToString());

                if ((bool)save.ShowDialog())
                {
                    domains = LoadDomains();

                    HtmlDocument doc = new HtmlDocument();

                    try
                    {
                        foreach (var file in Items.SelectedItems)
                        {
                            HtmlNode content;
                            HtmlAttribute attribute = null;

                            doc.Load(file.ToString());

                            string domain = GetDomain(doc);
                            string tag = GetContentTag(domains, domain);

                            //If the tag is not empty then we know what we are searching for so we use it
                            if (!string.IsNullOrEmpty(tag))
                            {
                                content = doc.DocumentNode.Descendants()
                                   .Where(x => x.Id == tag).FirstOrDefault();
                            }
                            else
                            {
                                //Grab content tag. Clearly this is unlikely to work for everything.
                                content = doc.DocumentNode.Descendants()
                                    .Where(x => x.Id.Contains("main") || x.Id.Contains("article") || x.Id.Contains("content") || x.Id.Contains("container")).FirstOrDefault();
                                //.Where(x => x.Id.Contains("article") || x.Id.Contains("content") || x.Id.Contains("container")).FirstOrDefault();
                                //.Where(x => x.Id.Contains("container")).FirstOrDefault();
                                //.Where(x => x.Attributes.Select(y => y.Name == "href").FirstOrDefault()).FirstOrDefault();
                                //.Where(x => x.Name == "link").FirstOrDefault();
                            }


                            foreach (var item in doc.DocumentNode.Descendants().Where(x => x.Name == "section"))
                            {
                                attribute = item.Attributes.Where(x => x.Name == "class" && x.Value == "body").FirstOrDefault();

                                if (attribute != null)
                                {
                                    content = item;
                                    break;
                                }
                            }

                            if (content != null)
                            {
                                //Remove all the nasties and Save to File
                                PurifyAndSave(save.FileName, content, doc.Encoding);
                            }
                            else
                            {
                                //Remove all the nasties and Save to File
                                PurifyAndSave(save.FileName, doc.DocumentNode, doc.Encoding);
                            }

                            Process.Start(@"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
                            string.Format("\"{0}\"", save.FileName));

                            if (MessageBoxResult.Yes == MessageBox.Show("Is it ok?", "OK", MessageBoxButton.YesNo))
                            {
                                if (string.IsNullOrEmpty(content.Id))
                                {
                                    AddDomainToFile(content.Id, domain);
                                }
                                else
                                {
                                    AddDomainToFile(domain, content.Name, attribute.Name, attribute.Value);
                                }
                            }
                            else
                            {
                                Process.Start(@"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
                     string.Format("\"{0}\"", file));

                                ImproveMe cc = new ImproveMe(domain);
                                cc.ShowDialog();

                            }

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

        public static void AddDomainToFile(string Id, string domain)
        {
            XElement site = new XElement("name",
                new XAttribute("domain", domain),
                new XAttribute("content", Id));

            domains.Root.Add(site);

            domains.Save(ConfigurationManager.AppSettings["path"]);
        }

        public static void AddDomainToFile(string domain, string tag, string type, string content)
        {
            XElement site = new XElement("name",
                new XAttribute("domain", domain),
                new XAttribute("tag", tag),
                new XAttribute("type", type),
                new XAttribute("content", content ));

            domains.Root.Add(site);

            domains.Save(ConfigurationManager.AppSettings["path"]);
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

        private static string GetDomain(HtmlDocument doc)
        {
            var domain = doc.DocumentNode.ChildNodes
                .Where(x => x.Name.Contains("comment") && x.InnerHtml.Contains("saved from"))
                .FirstOrDefault().InnerHtml;

            domain = Regex.Match(domain, @"([^:]*:\/\/)?([^\/]+\.[^\/]+)").Groups[2].Value;

            return domain;
        }

        private static string GetContentTag(XDocument domains, string domain)
        {
            string result = string.Empty;

            var tag = domains.Root.Descendants()
                .Where(x => x.Name == domain)
                .Select(x => x.Attribute("content")).FirstOrDefault();

            if (tag != null)
            {
                result = tag.ToString();
            }

            return result;
        }

        private static XDocument LoadDomains()
        {
            return XDocument.Load(ConfigurationManager.AppSettings["path"]);
        }

        private static void PurifyAndSave(string fileName, HtmlNode content, Encoding encoding)
        {
            content.Descendants()
                .Where(x => x.Name == "script" || x.Name == "iframe" || x.Name == "noscript" || x.Name == "form" || x.Id.Contains("comment")).ToList()
                .ForEach(x => x.Remove());

            using (StreamWriter sw = new StreamWriter(fileName, true, encoding))
            {
                sw.Write(content.InnerHtml.Replace("â€™", "'").Replace("â€œ", "\"").Replace("â€", "\""));
            }
        }


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
