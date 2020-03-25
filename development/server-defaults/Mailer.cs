using System;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;

using Serilog;
using Serilog.Core;
using Newtonsoft.Json.Linq;
using Controllers;

namespace Common
{
    public class Mailer
    {
        public Mailer(Logger log)
        {
            this.log = log;
            Init();
        }
        public Logger log;
        private string GmailServer = "smtp.gmail.com";
        private int GmailPort = 587;
        private string mailAddress;
        private string mailReceiver;
        private string mailPassword;
        private bool emailEnable;
        private MailAddress from;
        private SmtpClient smtp;

        public void Init()
        {
            var config = Program.serverConfiguration();
            mailAddress = config.GetValue<string>("mail_address");
            mailPassword = config.GetValue<string>("mail_password");
            GmailServer = config.GetValue<string>("smtp_server");
            GmailPort = config.GetValue<int>("smtp_port");
            emailEnable = config.GetValue<bool>("email_enable");
            mailReceiver = config.GetValue<string>("mail_receiver");
            smtp = new SmtpClient(GmailServer, GmailPort);
            smtp.Credentials = new NetworkCredential(mailAddress, mailPassword);
            from = new MailAddress(mailAddress, "Contact Us");
            smtp.EnableSsl = true;
        }
        public async void SendEmail(string email, string subject, string text)
        {
            MailAddress to = new MailAddress(email);
            MailMessage message = new MailMessage(from, to);
            message.Subject = subject;
            message.Body = text;
            message.IsBodyHtml = true;
            try {
                if (emailEnable)
                    await smtp.SendMailAsync(message);
                log.Information("Send message to " + email);
            }
            catch (Exception e) {
                log.Error("Can't send email message, ex: " + e.Message);
            }
        }
        public async void ContactUsReceiver(ContactUsCache cache)
        {
            MailAddress to = new MailAddress(mailReceiver);
            MailMessage message = new MailMessage(from, to);
            message.Subject = "Contact us!";
            message.Body = 	"Name: " + cache.message_name + "\r\n" +
                "Email: " + cache.message_email + "\r\n" +
                "Company Name: " + cache.message_company_name + "\r\n" +
                "Budget: " + cache.message_budget + "\r\n" +
                "Description: " + cache.message_description + "\r\n";
            message.IsBodyHtml = true;
            try {
                if (emailEnable)
                    await smtp.SendMailAsync(message);
                log.Information("Contact us from " + cache.message_email);
            }
            catch (Exception e) {
                log.Error("Can't execute 'contact us' method, message -> " + e.Message);
            }
        }
    }
}
