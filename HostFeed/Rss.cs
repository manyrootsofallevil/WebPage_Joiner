using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Syndication;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

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
        public Rss20FeedFormatter GetBlog()
        {
            SyndicationFeed feed = new SyndicationFeed("My Blog Feed", "This is a test feed", new Uri("http://ManyRootsofAllEvil.blogger.com"));
            feed.Authors.Add(new SyndicationPerson("ManyRootsofAllEvil@ManyRootsofAllEvil.com"));
            feed.Categories.Add(new SyndicationCategory("Interesting articles"));
            feed.Description = new TextSyndicationContent("Articles");

            SyndicationItem item1 = new SyndicationItem(
                "Item One",
                "This is the content for item one",
                new Uri("http://www.isitajoke.de/2013/01/19-beef-meat-label-overseeing-task-of.html"),
                "ItemOneID",
                DateTime.Now);

            SyndicationItem item2 = new SyndicationItem(
                "Item Two",
                "This is the content for item two",
                new Uri("http://arstechnica.com/information-technology/2013/02/100gbps-and-beyond-what-lies-ahead-in-the-world-of-networking/"),
                "ItemTwoID",
                DateTime.Now);

            SyndicationItem item3 = new SyndicationItem(
                "Item Three",
                "This is the content for item three",
                new Uri("http://www.technologyreview.com/news/507971/welcome-to-the-malware-industrial-complex/"),
                "ItemThreeID",
                DateTime.Now);

            List<SyndicationItem> items = new List<SyndicationItem>();

            items.Add(item1);
            items.Add(item2);
            items.Add(item3);

            feed.Items = items;

            return new Rss20FeedFormatter(feed);
        }
    }
}
