using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace MindSphereAuthAPI.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var fromEmail = _config["EmailSettings:FromEmail"];
            var fromPassword = _config["EmailSettings:AppPassword"];

            using (var smtpClient = new SmtpClient("smtp.gmail.com"))
            {
                smtpClient.Port = 587;
                smtpClient.Credentials = new NetworkCredential(fromEmail, fromPassword);
                smtpClient.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false,
                };
                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
            }
        }

        public async Task SendSessionLinkAsync(string clientEmail, string counsellorEmail, string sessionLink, string dateTime)
        {
            // Client email content
            string clientSubject = "Your Counseling Session Link";
            string clientBody = $"Hello,\n\nYour counseling session has been scheduled.\n" +
                                $"Date & Time: {dateTime}\n" +
                                $"Join here: {sessionLink}\n\n" +
                                $"Best regards,\nMindSphere";

            // Counsellor email content
            string counsellorSubject = "New Counseling Session Scheduled";
            string counsellorBody = $"Hello Counsellor,\n\nA new counseling session has been booked.\n" +
                                    $"Date & Time: {dateTime}\n" +
                                    $"Session Link: {sessionLink}\n\n" +
                                    $"Please be prepared for the session.\n\n" +
                                    $"Best regards,\nMindSphere";

            // Send to both
            await SendEmailAsync(clientEmail, clientSubject, clientBody);
            await SendEmailAsync(counsellorEmail, counsellorSubject, counsellorBody);
        }
    }
}
