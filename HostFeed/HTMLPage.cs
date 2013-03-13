using HtmlAgilityPack;
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
        private string fileName, directory, extension, url, title, content;
        HtmlDocument docx; // = new HtmlDocument();

        public HTMLPage(string fileName, string directory, string extension)
        {
            this.fileName = fileName;
            this.directory = directory.EndsWith("\\") ? directory : string.Concat(directory, "\\");
            this.extension = extension;
            getUrl();
        }

        public HTMLPage(string name)
        {
            this.fileName = Path.GetFileNameWithoutExtension(name);
            this.directory = string.Format(@"{0}\", Path.GetDirectoryName(name));
            this.extension = Path.GetExtension(name);
            getUrl();
            getContent();
        }

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }
        public string Directory { get; set; }
        public string Extension { get; set; }
        public string Url
        {
            get { return url; }
            set
            {
                Url = value;
            }
        }
        public string Title { get { return title; } set { Title = value; } }
        public string Content
        {
            get { return content; }
            set { Content = value; }
        }

        //By default this is the method used when binding.
        public override string ToString()
        {
            return fileName;
        }

        public string GetPage()
        {
            return string.Format("{0}{1}{2}", directory, fileName, extension);
        }

        private void getUrl()
        {

            if (docx == null)
            {
                docx = new HtmlDocument();
                docx.Load(this.GetPage(), System.Text.Encoding.UTF8, false);
            }
            var comment = docx.DocumentNode.ChildNodes
            .Where(x => x.Name.Contains("comment") && x.InnerHtml.Contains("saved from"))
            .FirstOrDefault();

            if (comment != null)
            {
                this.url = comment.InnerText.Split(')')[1].Remove(comment.InnerText.Split(')')[1].Length - 3).Trim();
            }
            else
            {
                throw new Exception("Could not find url");
            }
        }

        private void getContent()
        {
            if (docx == null)
            {
                docx = new HtmlDocument();
                docx.Load(this.GetPage(), System.Text.Encoding.UTF8, false);
            }
            //TODO: look for more content.
            var temp = docx.DocumentNode.DescendantsAndSelf()
                .Where(x => x.Name == "meta" && x.Attributes.Contains("name")
                    && x.Attributes.Where(y => y.Value == "description").Count() == 1)
                .FirstOrDefault();

                this.content = temp != null ? 
                    temp.Attributes.Where(x => x.Name == "content").FirstOrDefault().Value: string.Empty;


        }
    }
}
