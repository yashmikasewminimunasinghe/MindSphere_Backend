using System.Net;
using System.Net.Mail;

public static class EmailService
{
    public static async Task SendSessionLinkAsync(string email, string sessionLink, DateTime sessionDateTime, string counsellorName, string recipientName)
    {
        var smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential("your-email@gmail.com", "your-app-password"),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress("your-email@gmail.com"),
            Subject = "Counseling Session Confirmation",
            Body = $@"
                <p>Hi {recipientName},</p>
                <p>Your counseling session has been confirmed.</p>
                <p><strong>Session Details:</strong></p>
                <ul>
                    <li><strong>Date & Time:</strong> {sessionDateTime.ToString("f")}</li>
                    <li><strong>Counsellor:</strong> {counsellorName}</li>
                    <li><strong>Session Link:</strong> <a href='{sessionLink}'>{sessionLink}</a></li>
                </ul>
                <p>Please join on time. Thank you for choosing MindSphere.</p>",
            IsBodyHtml = true,
        };

        mailMessage.To.Add(email);

        await smtpClient.SendMailAsync(mailMessage);
    }
}
