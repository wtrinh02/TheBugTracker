using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using TheBugTracker.Models;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace TheBugTracker.Services
{
    public class BTEmailService : IEmailSender
    {
        private readonly MailSettings _mailSettings;
        public BTEmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }
        public async Task SendEmailAsync(string emailTo, string subject, string htmlMessage)
        {
            MimeMessage email = new();

            email.Sender = MailboxAddress.Parse(_mailSettings.Mail ?? Environment.GetEnvironmentVariable("Mail"));
            email.To.Add(MailboxAddress.Parse(emailTo));
            email.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = htmlMessage
            };

            email.Body = builder.ToMessageBody();
            try
            {
                using var smtp = new SmtpClient();

                var host = _mailSettings.MailHost ?? Environment.GetEnvironmentVariable("MailHost");
                var port = _mailSettings.MailPort != 0 ? _mailSettings.MailPort : int.Parse(Environment.GetEnvironmentVariable("MailPort")!);
                var password = _mailSettings.MailPassword ?? Environment.GetEnvironmentVariable("MailPassword");
                var mail = _mailSettings.Mail ?? Environment.GetEnvironmentVariable("Mail");

                smtp.Connect(host, port, SecureSocketOptions.StartTls);
                smtp.Authenticate(mail,password);

                await smtp.SendAsync(email);

                smtp.Disconnect(true);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
