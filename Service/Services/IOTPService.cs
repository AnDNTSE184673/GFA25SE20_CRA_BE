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
        Task<string> SendOTPCodes(Guid userId);
        Task<string> SubmitOTPCodes(Guid userId, string unhashedCode);
    }
}
