using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Repositories.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

namespace Services.Service.Implementation
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly SMTPSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, IOptions<SMTPSettings> settings, ILogger<EmailService> logger)
        {
            _config = config;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string subject, string body, string receiverMail, string? receiverName)
        {
            bool status = false;
            /*var host = _config["MailSettings:Host"];
            var port = int.Parse(_config["MailSettings:Port"]);
            var from = _config["MailSettings:SenderEmail"];
            var password = _config["MailSettings:Password"];*/

            //_settings object is set from the middleware in dependancy injection (service.Configure)

            var emailSend = new MimeMessage();
            emailSend.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            emailSend.To.Add(new MailboxAddress(receiverName, receiverMail));

            emailSend.Subject = subject;
            try
            {
                emailSend.Body = new TextPart("html")
                {
                    Text = body
                };

                using (var smtp = new SmtpClient())
                {
                    await smtp.ConnectAsync(_settings.Host, _settings.Port);
                    await smtp.AuthenticateAsync(_settings.Username, _settings.Password);

                    await smtp.SendAsync(emailSend);

                    await smtp.DisconnectAsync(true);
                }
                status = true;
            }
            catch(Exception ex)
            {
                //_logger.LogError(ex, "An error occurred while sending the email.");
                status = false;
                throw new Exception(ex.Message);
            }
            return status;
        }

        public string GenerateBodyRegisterSuccess(string username, string? password)
        {


            StringBuilder mailBody = new StringBuilder();

            mailBody.AppendLine("<div style='font-family: Arial, sans-serif; color: #000000;'>");
            mailBody.AppendLine("<h1 style='color: #000000;'>Account Confirmation</h1>");

            mailBody.AppendLine($"<p style='color: #000000;'>Hi <strong>{username}</strong>, you've received this email because an account with this email has been successfully registered on YuuZone.</p>");
            mailBody.AppendLine("<br/>");
            mailBody.AppendLine($"<h3 style='color: #000000;'>Account Information:</h3>");
            mailBody.AppendLine($"<h4 style='color: #000000;'>NOTE: If the password box is empty, that usually means this account was created via Google</h4>");
            mailBody.AppendLine($"<p style='color: #000000;'> Username: <strong>{username}</strong> </p>");
            mailBody.AppendLine($"<p style='color: #000000;'> Password (highlight to view): <span style='background-color: black; color: transparent;'>{password}</span> </p>");
            

            //put email verifying jwt here
            /*mailBody.AppendLine($"<a href='[url]' style=' display: block; text-align: center; font-weight: bold; background-color: #008CBA; font-size: 16px; border-radius: 10px; color: #ffffff; cursor: pointer; width: 50%; padding: 10px; text-decoration: none;'>");
            mailBody.AppendLine("Confirm Email");
            mailBody.AppendLine("</a>");*/

            mailBody.AppendLine("<p style='color: #000000;'> If you didn't create this account, please contact us via our email: (contact here). </p>");
            mailBody.AppendLine("<h5 style='color: #000000;'>Best regards,<br>YuuZone Team</h5>");
            mailBody.AppendLine("</div>");

            return mailBody.ToString();

        }

        /*public string GenerateBodyConfirmEmail(string username, string password)
        {


            StringBuilder mailBody = new StringBuilder();

            mailBody.AppendLine("<div style='font-family: Arial, sans-serif; color: #000000;'>");
            mailBody.AppendLine("<h1 style='color: #000000;'>Account Confirmation</h1>");

            mailBody.AppendLine($"<p style='color: #000000;'>Hi <strong>{username}</strong>, you've received this email because an account with this email has been successfully registered on YuuZone</p>");
            mailBody.AppendLine("<br/>");
            mailBody.AppendLine($"<h4 style='color: #000000;'>Account Information:</h4>");
            mailBody.AppendLine($"<p style='color: #000000;'> Username: <strong>{username}</strong> </p>");
            mailBody.AppendLine($"<p style='color: #000000;'> Password (highlight to view): <span style='background-color: black; color: transparent;'>{password}</span> </p>");
            

            //put email verifying jwt here
            /*mailBody.AppendLine($"<a href='[url]' style=' display: block; text-align: center; font-weight: bold; background-color: #008CBA; font-size: 16px; border-radius: 10px; color: #ffffff; cursor: pointer; width: 50%; padding: 10px; text-decoration: none;'>");
            mailBody.AppendLine("Confirm Email");
            mailBody.AppendLine("</a>");*/
            /*
            mailBody.AppendLine("<p style='color: #000000;'> If you didn't create this account, please contact us via our email (contact here) </p>");
            mailBody.AppendLine("<h5 style='color: #000000;'>Best regards,<br>YuuZone Team</h5>");
            mailBody.AppendLine("</div>");
            return mailBody.ToString();

    }*/
    }
}
