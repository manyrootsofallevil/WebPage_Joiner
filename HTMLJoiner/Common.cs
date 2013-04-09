using HostFeed;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTMLJoiner
{
    public enum AppType { Browser, EbookConverter, EbookViewer };

    public static class Common
    {
        public static void LoadFiles(ObservableCollection<HTMLPage> ItemList)
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
                    ItemList.Add(new HTMLPage(st));
                }
            }
        }

        public static List<string> LoadFiles()
        {
            List<string> result = null;

            OpenFileDialog files = new OpenFileDialog();
            files.AddExtension = true;
            files.CheckFileExists = true;
            files.Multiselect = true;
            files.Filter = "Txt Files|*.txt";
            files.FilterIndex = 1;

            if ((bool)files.ShowDialog())
            {
                result = files.FileNames.ToList<string>();
            }

            return result;
        }

        public static SaveFileDialog InstatiateSaveDialog(bool multiple)
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

        public static bool RunExternalApplication(AppType app, string arguments)
        {
            bool output = false;

            switch (app)
            {
                case AppType.Browser:
                    Process.Start(ConfigurationManager.AppSettings["Browser"], arguments);                   
                    break;
                case AppType.EbookConverter:
                    //Process.Start(, arguments);
                    var proc = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = ConfigurationManager.AppSettings["EbookConverter"],
                            Arguments = arguments,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    };
                    proc.Start();
                    while (!proc.StandardOutput.EndOfStream)
                    {
                        string line = proc.StandardOutput.ReadLine().ToLower();
                        if (line.Contains("output saved"))
                        {
                            output=true;
                        }
                    }
                    break;
                case AppType.EbookViewer:
                    Process.Start(ConfigurationManager.AppSettings["EbookViewer"],arguments);
                    break;

                   

            }
            //TODO: Have a look at this to get the ouput from ebook converter process
            return output;
        }

        public static void EncryptAppSettings()
        {
            // Get the current configuration file.
            Configuration config =
                    ConfigurationManager.OpenExeConfiguration(System.Reflection.Assembly.GetExecutingAssembly().Location);

            ConfigurationSection section = config.GetSection("appSettings");

            if (!section.SectionInformation.IsProtected)
            {
                // Protect (encrypt)the section.
                section.SectionInformation.ProtectSection("RsaProtectedConfigurationProvider");

                // Save the encrypted section.
                section.SectionInformation.ForceSave = true;

                config.Save(ConfigurationSaveMode.Full);
            }
        }

    }
}
