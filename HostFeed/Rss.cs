using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Syndication;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HostFeed
{
    [ServiceContract]
    public interface IBlog
    {
        [OperationContract]
        [WebGet]
        Rss20FeedFormatter GetBlog();


    }

    public class BlogService : IBlog
    {
        static List<SyndicationItem> items = new List<SyndicationItem>();
        SyndicationFeed feed;

        public Rss20FeedFormatter GetBlog()
        {
            feed = feed ?? getBlog();

            return new Rss20FeedFormatter(feed);

        }

        private SyndicationFeed getBlog()
        {
            SyndicationFeed feed = new SyndicationFeed("My Blog Feed", "This is a test feed", new Uri("http://ManyRootsofAllEvil.blogger.com"));
            feed.Authors.Add(new SyndicationPerson("ManyRootsofAllEvil@ManyRootsofAllEvil.com"));
            feed.Categories.Add(new SyndicationCategory("Interesting articles"));
            feed.Description = new TextSyndicationContent("Articles");

            items = new List<SyndicationItem>();

            XDocument xdoc = XDocument.Load(ConfigurationManager.AppSettings["rsssource"]);

            foreach (XElement element in xdoc.Root.Descendants())
            {
                items.Add(new SyndicationItem(element.Attribute("title").Value,
                    element.Attribute("content").Value, new Uri(element.Attribute("URI").Value)));
            }

            feed.Items = items;

            return feed;
        }



        private bool GetItems(List<HTMLPage> Pages)
        {
            //List<SyndicationItem> items = new List<SyndicationItem>();

            foreach (HTMLPage page in Pages)
            {
                items.Add(new SyndicationItem(page.Title, page.Content, new Uri(page.Url)));

            }

            //return items;
            return true;
        }
    }
}
