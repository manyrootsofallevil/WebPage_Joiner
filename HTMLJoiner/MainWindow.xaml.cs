using HtmlAgilityPack;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
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

namespace HTMLJoiner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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

                    HtmlDocument doc = new HtmlDocument();
                   
                    try
                    {
                        foreach (var file in Items.SelectedItems)
                        {
                            doc.Load(file.ToString());

                            //Grab content tag. Clearly this is unlikely to work for everything.
                            HtmlNode content = doc.DocumentNode.Descendants()
                                //.Where(x => x.Id.Contains("main") || x.Id.Contains("article") || x.Id.Contains("content") || x.Id.Contains("container")).FirstOrDefault();
                                .Where(x => x.Id.Contains("article") || x.Id.Contains("content") || x.Id.Contains("container")).FirstOrDefault();

                            //HtmlNode content = FindInner(doc.DocumentNode);

                            if (content !=null)
                            {
                                //Remove all the nasties
                                content.Descendants()
                                    .Where(x => x.Name == "script" || x.Name == "iframe" || x.Name == "noscript" || x.Name == "form" || x.Id.Contains("comment")).ToList()
                                    .ForEach(x => x.Remove());
                                
                                using (StreamWriter sw = new StreamWriter(save.FileName, true, doc.Encoding))
                                {
                                    sw.Write(content.InnerHtml.Replace("â€™", "'").Replace("â€œ", "\"").Replace("â€", "\""));
                                }
                            }
                            else
                            {
                                doc.DocumentNode.Descendants()
                                .Where(x => x.Name == "script" || x.Name == "iframe" || x.Name == "noscript" || x.Name == "form" || x.Id.Contains("comment")).ToList()
                                .ForEach(x => x.Remove());

                                using (StreamWriter sw = new StreamWriter(save.FileName, true, doc.Encoding))
                                {
                                    sw.Write(doc.DocumentNode.InnerHtml.Replace("â€™", "'").Replace("â€œ", "\"").Replace("â€", "\""));
                                }
                                
                            }

                        }

                        MessageBox.Show("success");
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
                for (int i=0; i < Items.SelectedItems.Count; i++)
                {
                    ItemList.Remove(Items.SelectedItems[i].ToString());
                }
            }

        }


        private HtmlNode FindInner(HtmlNode node)
        {
            HtmlNode result = null;

            HtmlNode content = node.Descendants()
            .Where(x => x.Id.Contains("main") || x.Id.Contains("article") || x.Id.Contains("content") || x.Id.Contains("container") ).FirstOrDefault();

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

    }
}
