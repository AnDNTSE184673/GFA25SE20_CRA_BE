using Microsoft.AspNetCore.Http;
using Repository.DTO.ResponseDTO.DriverLicense;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public interface IFPTAIService
    {
        Task<DriverLincenseInfo> ExtractDriverLicenseInfo(IFormFile image);
    }
}
