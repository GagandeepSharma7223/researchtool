using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace chapterone.email.sendgrid
{
    public class SendGridEmailService : IEmailService
    {
        private const string NO_REPLY_EMAIL = "noreply@chapterone.la";
        private const string NO_REPLY_NAME = "Research Library";

        private readonly SendGridClient _client;


        /// <summary>
        /// Constructor
        /// </summary>
        public SendGridEmailService(string apiKey)
        {
            _client = new SendGridClient(apiKey);
        }


        /// <summary>
        /// Send the given email text as HTML
        /// </summary>
        public async Task<bool> SendEmail(string email, string subject, string html)
        {
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(NO_REPLY_EMAIL, NO_REPLY_NAME),
                Subject = subject,
                HtmlContent = html
            };
                
            msg.AddTo(new EmailAddress(email));
                
            var response = await _client.SendEmailAsync(msg);

            return response.StatusCode == HttpStatusCode.Accepted;
        }
    }
}
