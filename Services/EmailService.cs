using System.Net;
using System.Net.Mail;

namespace PaymentBE.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            var fromEmail = smtpSettings.GetValue<string>("From");
            var smtpHost = smtpSettings.GetValue<string>("Host");
            var smtpPort = smtpSettings.GetValue<int>("Port");
            var smtpUsername = smtpSettings.GetValue<string>("Username");
            var smtpPassword = smtpSettings.GetValue<string>("Password");

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,  // Set to false if you don't want HTML content
            };

            mailMessage.To.Add(to);

            using (var smtpClient = new SmtpClient(smtpHost, smtpPort))
            {
                smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                smtpClient.EnableSsl = true;  // Enable SSL for security
                await smtpClient.SendMailAsync(mailMessage);
            }
        }
    }


}
