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
    //2. Create Chrome Extension that saves urls in file [wouldn't be simpler to save them as bookmarks? use sqllite
    //bookmarks here C:\Users\[YourUserName]\AppData\Local\Google\Chrome\User Data\Default\] 
    //so that thye can be feed to 1.
    // 3. Convert individual files to mobi, is this needed?

    //TODO: Add a waiting thingy while processing
    //TODO: Use MVVM pattern

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static XDocument domains;

        private CollectionViewSource fileList = new CollectionViewSource();

        private readonly BackgroundWorker feed = new BackgroundWorker();
        private readonly BackgroundWorker convert = new BackgroundWorker();
        private bool CompletedConversion = false;

        public MainWindow()
        {
            this.ItemList = new ObservableCollection<HTMLPage>();
            fileList.Source = ItemList;
            //this.fileList.SortDescriptions.Add(new SortDescription("HTMLPage.FileName",
            //   ListSortDirection.Ascending));

            InitializeComponent();

            Wait.IsEnabled = false;
            DataContext = this;

            feed.DoWork += (o, e) =>
            {
                Dispatcher.Invoke((Action)(() => Periodical.IsEnabled = false));

                Host.Start(); e.Result = true;
            };

            feed.RunWorkerCompleted += (o, e) =>
            {
                if ((bool)e.Result)
                {
                    convert.RunWorkerAsync();
                }
                else
                {
                    Dispatcher.Invoke((Action)(() => MessageBox.Show("An Error Occurred while initiating the local server")));
                    UpdateUIuponCompletion();
                }

            };

            convert.DoWork += (o, e) =>
                {
                    if (this.ItemList.Count() > 0)
                    {


                        string exePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

                        ImageModifier image = new ImageModifier(string.Format("{0}\\Images\\beastie.jpg", exePath));

                        image.WriteMessageToImage(DateTime.Now.ToLongDateString(), "modifiedbeastie.jpg");

                        CompletedConversion = Common.RunExternalApplication(AppType.EbookConverter,
                                   string.Format("{0} {1}{2}.mobi --authors {3} --title {4} --cover {5}",
                                   ConfigurationManager.AppSettings["newsrecipepath"],
                                   ConfigurationManager.AppSettings["savefilepath"],
                                   DateTime.Now.ToString("yyyyMMdd"),
                                   ConfigurationManager.AppSettings["author"],
                                   DateTime.Now.ToString("yyyyMMdd"),
                                   string.Format("\"{0}\\modifiedbeastie.jpg\"",exePath) ));

                        //Should really migrate the whole thing to use MVVM.
                        if (CompletedConversion)
                        {
                            Emailer.Hotmail(string.Format("{0}{1}.mobi", ConfigurationManager.AppSettings["savefilepath"],
                                   DateTime.Now.ToString("yyyyMMdd")));

                            if (Dispatcher.Invoke((Func<bool>)(() => (bool)Delete.IsChecked)))
                            {
                                foreach (var item in this.ItemList)
                                {
                                    if (item.FileName != item.Url)
                                    {
                                        File.Delete(item.GetPage());
                                    }
                                }
                            }

                            Dispatcher.Invoke((Action)(() => Items.DataContext = null));

                            Dispatcher.Invoke((Action)(() => this.ItemList = new ObservableCollection<HTMLPage>()));

                            if (Dispatcher.Invoke((Func<bool>)(() => (bool)OpenBook.IsChecked)))
                            {
                                Common.RunExternalApplication(AppType.EbookViewer,
                                    string.Format("{1}{0}.mobi", DateTime.Now.ToString("yyyyMMdd"), ConfigurationManager.AppSettings["savefilepath"]));
                            }
                            else
                            {
                                Dispatcher.Invoke((Action)(() =>
                                    MessageBox.Show(string.Format("Converted book can be found here {1}{0}.mobi", DateTime.Now.ToString("yyyyMMdd")
                                    , ConfigurationManager.AppSettings["savefilepath"]))));
                            }
                        }


                    }
                };

            convert.RunWorkerCompleted += (o, e) =>
                {
                    UpdateUIuponCompletion();
                };


        }

        private void UpdateUIuponCompletion()
        {
            Dispatcher.Invoke((Action)(() => Periodical.IsEnabled = true));
            Dispatcher.Invoke((Action)(() => Wait.Visibility = Visibility.Hidden));
            this.IsEnabled = true;
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

        #region events
        private void Periodical_Click(object sender, RoutedEventArgs e)
        {

            if (this.ItemList.Count() < 1)
            {
                try
                {
                    Common.LoadFiles(this.ItemList);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(ex.Message));
                }

            }

            Wait.Visibility = Visibility.Visible;
            this.IsEnabled = false;

            CreateRSSData mydata = new CreateRSSData();
            mydata.CreateRSSItems(ItemList);

            feed.RunWorkerAsync();

        }

        private void PeriodicalUrls_Click(object sender, RoutedEventArgs e)
        {
            if (this.ItemList.Count() < 1)
            {
                try
                {

                    List<string> files = Common.LoadFiles();

                    foreach (string file in files)
                    {
                        using (StreamReader sr = new StreamReader(file))
                        {
                            while (!sr.EndOfStream)
                            {
                                ItemList.Add(new HTMLPage(sr.ReadLine(), true));
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

            }

            Wait.Visibility = Visibility.Visible;
            this.IsEnabled = false;

            CreateRSSData mydata = new CreateRSSData();
            mydata.CreateRSSItems(ItemList);

            feed.RunWorkerAsync();
        }

        private void LoadFiles_Click(object sender, RoutedEventArgs e)
        {
            Common.LoadFiles(this.ItemList);
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {

            Common.LoadFiles(this.ItemList);

            SaveFileDialog save = Common.InstatiateSaveDialog(ItemList.Count() > 1 ? true : false);

            if ((bool)save.ShowDialog())
            {

                try
                {
                    HtmlDocument doc = new HtmlDocument();

                    //foreach (string file in Items.SelectedItems)
                    foreach (HTMLPage page in FileList.View)
                    {
                        SaveHTMLToFile(save, doc, page.GetPage());
                    }



                    string saveFile = string.Format(@"{0}\{1}", System.IO.Path.GetDirectoryName(save.FileName),
                        System.IO.Path.GetFileNameWithoutExtension(save.FileName));

                    Common.RunExternalApplication(AppType.EbookConverter,
                        string.Format("{0} {1}.mobi --authors {2} --title {3}", save.FileName, saveFile, "YesMen",
                        string.Format("Summary {0}", DateTime.Now.ToString("yyyyMMdd"))));

                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Error: {0} - {1}.", ex, ex.GetType()));
                }
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
                    //ItemList.Remove();
                }
            }

        } 
        #endregion

        #region methods
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
                Common.RunExternalApplication(AppType.Browser, string.Format("\"{0}\"", save.FileName));
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
                Common.RunExternalApplication(AppType.Browser, string.Format("\"{0}\"", file));
                //Run the Improve Me Dialog.
                ImproveMe cc = new ImproveMe(domain);

                if ((bool)cc.ShowDialog())
                {
                    //Try Again;
                    SaveHTMLToFile(save, doc, file);
                }
            }
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
                    sw.Write(content.InnerHtml);
                    //Calibre will assume a new page whenever it finds an H1 tag, so this will ensure that articles are separated
                    sw.WriteLine("<H1></H1>");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }



        private static bool CheckDomainExists(XDocument domains, string domain)
        {
            return domains.Root.Descendants().Where(x => x.Name == domain).Count() == 1;
        }
        
        #endregion


    }
}
