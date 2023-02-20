using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using DotNetEnv;
using System.IO;

namespace verifyMail
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DotNetEnv.Env.Load(@"../../../.env");
            string senderEmail = "alhambra.noreply@gmail.com";
            string senderPassword = Environment.GetEnvironmentVariable("AlhambraEmailPassword");
            Console.Write("Recipient email: ");
            string recipientEmail = Console.ReadLine();
            string subject = "Test email";
            string body = "<h1>Verify your email</h1>";

            var smtpClient = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true
            };

            var message = new MailMessage(senderEmail, recipientEmail, subject, body);
            message.IsBodyHtml = true;

            smtpClient.Send(message);
        }
    }
}
