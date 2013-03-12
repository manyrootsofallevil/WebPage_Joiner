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
    public enum AppType { Browser, EbookConverter };

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

        public static void RunExternalApplication(AppType app, string arguments)
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

    }
}
