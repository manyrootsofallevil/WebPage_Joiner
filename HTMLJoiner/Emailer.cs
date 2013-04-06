using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace HTMLJoiner
{
    public static class Emailer
    {
        public static void Hotmail(string fileName)
        {
            SmtpClient SmtpServer = new SmtpClient("smtp.live.com");
            var mail = new MailMessage();
            mail.From = new MailAddress("@hotmail.com");
            mail.To.Add("@kindle.com");
            mail.Subject = DateTime.Now.ToLongDateString(); ;
            mail.Attachments.Add(new Attachment(fileName));
            SmtpServer.Port = 587;
            SmtpServer.UseDefaultCredentials = false;
            SmtpServer.Credentials = new System.Net.NetworkCredential("@hotmail.com", "");
            SmtpServer.EnableSsl = true;
            SmtpServer.Send(mail);
        }
    }
}
