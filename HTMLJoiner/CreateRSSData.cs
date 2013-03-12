using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HostFeed;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.Configuration;

namespace HTMLJoiner
{
    public class CreateRSSData
    {
        public void CreateRSSItems(ObservableCollection<HTMLPage> pages)
        {
            XDocument xdoc = new XDocument();
            xdoc.Add(new XElement("Root"));

            foreach (HTMLPage page in pages)
            {
                xdoc.Root.Add(new XElement("RssItem", new XAttribute("title", page.FileName)
                , new XAttribute("content", page.Content), new XAttribute("URI", page.Url)));
            }

            xdoc.Save(ConfigurationManager.AppSettings["rsssource"]);
        }
    }
}
