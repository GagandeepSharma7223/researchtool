using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chapterone.services.interfaces
{
    public interface ICustomEmailService
    {
        Task SendEmailAsync(string email, string subject, string message, bool htmlContent = true);
    }
}
