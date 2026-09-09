using System;
using System.Collections.Generic;
using System.Linq;
using MailKit.Net.Smtp;
using System.Text;
using System.Threading.Tasks;
using BLL.Interfaces;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using BLL.Settings.Email;

namespace BLL.Services
{
    public class SMTPEmailService : IEmailService
    {
        private readonly IOptions<EmailSettings> _emailSettings;
        public SMTPEmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings;
        }
        public async Task SendEmailAsync(Email email)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_emailSettings.Value.SenderName, _emailSettings.Value.SenderEmail));
            emailMessage.To.Add(MailboxAddress.Parse(email.To));
            emailMessage.Subject = email.Subject;

            emailMessage.Body = new TextPart(TextFormat.Html)
            {
                Text = email.Body
            };
            try
            {
                using var smtp = new SmtpClient();
                //connect
                await smtp.ConnectAsync(_emailSettings.Value.SmtpServer, _emailSettings.Value.SmtpPort, SecureSocketOptions.StartTls);
                //authenticate
                await smtp.AuthenticateAsync(_emailSettings.Value.SenderEmail, _emailSettings.Value.Password);

                //send email
                await smtp.SendAsync(emailMessage);
                //Disconnect
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Email sending failed. Please try again later.",
                    ex
                );
            }

            }
    }
}
