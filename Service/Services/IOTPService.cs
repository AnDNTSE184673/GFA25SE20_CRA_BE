using Repository.Data.Entities;
using Repository.DTO.ResponseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public interface IOTPService
    {
        Task<string> SendOTPCodes(Guid userId, string? additionalInfo);
        Task<string> ResendOTPCodes(Guid userId, string? additionalInfo);
        Task<(OTPCode entry, string message)> OTPVerificationAsync(string OTPCode, string email);
        Task<(OTPCode entry, string message)> SubmitOTPCodes(Guid userId, string unhashedCode);
    }
}
