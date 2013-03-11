using System;
using System.Collections.Generic;
using System.IO;
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
        static List<SyndicationItem> items = new List<SyndicationItem>();

        public Rss20FeedFormatter GetBlog()
        {
            SyndicationFeed feed = new SyndicationFeed("My Blog Feed", "This is a test feed", new Uri("http://ManyRootsofAllEvil.blogger.com"));
            feed.Authors.Add(new SyndicationPerson("ManyRootsofAllEvil@ManyRootsofAllEvil.com"));
            feed.Categories.Add(new SyndicationCategory("Interesting articles"));
            feed.Description = new TextSyndicationContent("Articles");

            List<SyndicationItem> items = new List<SyndicationItem>();
            int counter =1;

            using (StreamReader sr = new StreamReader(@"C:\urls.txt"))
            {
                while(sr.Peek() >= 0)
                {
                items.Add(new SyndicationItem(string.Format("item{0}",counter), string.Format("content{0}",counter),
                new Uri(sr.ReadLine())));
                counter++;
                }
            }


           // GetItems();

            //SyndicationItem item1 = new SyndicationItem(
            //    "Item One",
            //    "This is the content for item one",
            //    new Uri("http://www.isitajoke.de/2013/01/19-beef-meat-label-overseeing-task-of.html"),
            //    "ItemOneID",
            //    DateTime.Now);

            //SyndicationItem item2 = new SyndicationItem(
            //    "Item Two",
            //    "This is the content for item two",
            //    new Uri("http://arstechnica.com/information-technology/2013/02/100gbps-and-beyond-what-lies-ahead-in-the-world-of-networking/"),
            //    "ItemTwoID",
            //    DateTime.Now);

            //SyndicationItem item3 = new SyndicationItem(
            //    "Item Three",
            //    "This is the content for item three",
            //    new Uri("http://www.technologyreview.com/news/507971/welcome-to-the-malware-industrial-complex/"),
            //    "ItemThreeID",
            //    DateTime.Now);



            //items.Add(item1);
            //items.Add(item2);
            //items.Add(item3);

            feed.Items = items;

            return new Rss20FeedFormatter(feed);
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
