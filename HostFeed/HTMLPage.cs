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
            
            //Complete Save pages from Chrome will contain something like this
            //<!-- saved from url=(0024)http://www.schneier.com/ -->
            var comment = docx.DocumentNode.ChildNodes
            .Where(x => x.Name.Contains("comment") && x.InnerHtml.Contains("saved from"))
            .FirstOrDefault();

            //Failing that we look for <link rel=canoninal href="">
            var link = docx.DocumentNode.DescendantsAndSelf()
                .Where(x => x.Name == "link" 
                    && x.Attributes.Where(y=>y.Name=="rel" && y.Value=="canonical").Count()==1)
                .FirstOrDefault();

            //<meta property="og:url" content="http://www.alternet.org/silicon-valley-reportedly-full-stoners" />

            var meta = docx.DocumentNode.DescendantsAndSelf()
                .Where(x => x.Name == "meta"
                    && x.Attributes.Where(y => y.Name == "property" && y.Value == "og:url").Count() == 1)
                .FirstOrDefault();

            if (link !=null)
            {
                this.url = link.Attributes.Where(x=>x.Name=="href").FirstOrDefault().Value;

                //This is wrong in so many levels.
                try
                {
                   Uri test = new Uri(this.url);
                }
                catch
                {
                    if (meta !=null)
                    {
                        this.url = meta.Attributes.Where(x => x.Name == "content").FirstOrDefault().Value;
                    }
                }
            }
            else if(comment != null)
            {
                this.url = comment.InnerText.Split(')')[1].Remove(comment.InnerText.Split(')')[1].Length - 3).Trim();
            }
            else
            {
                throw new Exception(string.Format("Could not find url for {0}",this.GetPage()));
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
