using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System.Threading.Tasks;
using System;

namespace ChatApp.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var email = new MimeMessage();
                var senderName = _configuration["EmailSettings:SenderName"] ?? "ChatApp";
                var senderEmail = _configuration["EmailSettings:SenderEmail"] ?? throw new Exception("SenderEmail not configured");
                var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? throw new Exception("SmtpServer not configured");
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? throw new Exception("SmtpPort not configured"));
                var password = _configuration["EmailSettings:Password"] ?? throw new Exception("Password not configured");

                email.From.Add(new MailboxAddress(senderName, senderEmail));
                email.To.Add(new MailboxAddress("", toEmail));
                email.Subject = subject;
                email.Body = new TextPart("plain") { Text = body };

                using var smtp = new SmtpClient();
                Console.WriteLine($"Connecting to SMTP server: {smtpServer}:{smtpPort}");
                await smtp.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                Console.WriteLine($"Authenticating with {senderEmail}");
                await smtp.AuthenticateAsync(senderEmail, password);
                Console.WriteLine($"Sending email to {toEmail}");
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
                Console.WriteLine($"Email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email to {toEmail}: {ex.Message}, InnerException: {ex.InnerException?.Message}");
                throw new Exception($"Failed to send email: {ex.Message}", ex);
            }
        }
    }
}