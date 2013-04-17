using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace HTMLJoiner
{
    public static class Emailer
    {
        public static void Hotmail(string fileName)
        {
            string from =ConfigurationManager.AppSettings["FromEmail"];
            string to = ConfigurationManager.AppSettings["ToEmail"];

            SmtpClient SmtpServer = new SmtpClient("smtp.live.com");
            var mail = new MailMessage();
            mail.From = new MailAddress(from);
            mail.To.Add(to);
            mail.Subject = DateTime.Now.ToLongDateString(); ;
            mail.Attachments.Add(new Attachment(fileName));
            SmtpServer.Port = 587;
            SmtpServer.UseDefaultCredentials = false;
            SmtpServer.Credentials = new NetworkCredential(from, 
                    ConfigurationManager.AppSettings["password"]);

            Trace.TraceInformation(string.Format("From: {0} - To: {1}.", from, to));
            SmtpServer.EnableSsl = true;
            try
            {
                SmtpServer.Send(mail);
            }
            catch (Exception ex)
            {

                Trace.TraceError(ex.ToString());
            }
        }
    }
}
