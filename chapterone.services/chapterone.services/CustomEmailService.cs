using chapterone.services.interfaces;
using chapterone.shared;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace chapterone.services
{
    public class CustomEmailService : ICustomEmailService
    {
        private readonly ISendGridClient _sendGridClient;
        private readonly IConfiguration _configuration;
        public CustomEmailService(ISendGridClient sendGridClient, IConfiguration configuration)
        {
            _sendGridClient = sendGridClient;   
            _configuration = configuration; 
        }

        public async Task SendEmailAsync(string email, string subject, string message, bool htmlContent = true)
        {
            string fromEmail = _configuration[ConfigurationKeys.SendGrid__Email];
            string fromName = _configuration[ConfigurationKeys.SendGrid__User];
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(fromEmail, fromName),
                Subject = subject
            };
            if (htmlContent)
            {
                msg.HtmlContent = message;
            }
            else
            {
                msg.PlainTextContent = message;
            }
            msg.AddTo(new EmailAddress(email));
            await _sendGridClient.SendEmailAsync(msg);
        }

    }
}
