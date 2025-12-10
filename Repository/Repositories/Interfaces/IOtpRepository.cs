using Repository.Base;
using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories.Interfaces
{
    public interface IOtpRepository : IGenericRepository<OTPCode>
    {
        Task<(string status, OTPCode obj)> CreateOtpAsync(OTPCode otp);
        Task<OTPCode> FindSentOtpByUserAsync(Guid userId);
    }
}
