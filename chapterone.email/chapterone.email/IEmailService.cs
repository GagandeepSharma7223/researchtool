using System;
using System.Threading.Tasks;

namespace chapterone.email
{
    public interface IEmailService
    {
        Task<bool> SendEmail(string to, string subject, string html);
    }
}
