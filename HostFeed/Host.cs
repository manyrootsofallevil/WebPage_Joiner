using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Syndication;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HostFeed
{
    //Looks like this needs to run with elevated permissions
    //TODO: Sort this mess out.
    public static class Host
    {
        static Uri baseAddress = new Uri("http://localhost:8000/BlogService");
        static WebServiceHost svcHost = new WebServiceHost(typeof(BlogService), baseAddress);
        static bool opened = false;



        public static void Start()
        {

            svcHost.Opened += (o, e) => { opened = true; };
            svcHost.Closed += (o, e) => { opened = false; };

            try
            {
                if (!opened)
                {
                    svcHost.Open();
                }
            }
            catch (CommunicationException ce)
            {
               // MessageBox.Show("An exception occurred: {0}", ce.Message);
                svcHost.Abort();
            }
        }

        public static void Stop()
        {
            try
            {
                svcHost.Close();
            }
            catch (Exception ex)
            {
                
                throw;
            }
        }
    }
}
