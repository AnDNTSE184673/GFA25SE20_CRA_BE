using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Service
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string subject, string body, string receiverMail, string receiverName);
        string GenerateBodyRegisterSuccess(string username, string password, string productName, string? contactInfo);
    }
}
