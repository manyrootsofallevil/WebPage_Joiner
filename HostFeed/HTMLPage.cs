using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostFeed
{
    public class HTMLPage
    {
        private string fileName, directory, extension, url, title,content;

        public HTMLPage(string fileName, string directory, string extension)
        {
            this.fileName = fileName;
            this.directory = directory.EndsWith("\\") ? directory : string.Concat(directory, "\\");
            this.extension = extension;
        }

        public HTMLPage(string name)
        {
            this.fileName = Path.GetFileNameWithoutExtension(name);
            this.directory = string.Format(@"{0}\", Path.GetDirectoryName(name));
            this.extension = Path.GetExtension(name);
        }

        public string FileName { get; set; }
        public string Directory { get; set; }
        public string Extension { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public override string ToString()
        {
            return fileName;
        }

        public string GetPage()
        {
            return string.Format("{0}{1}{2}", directory, fileName, extension);
        }

    }
}
