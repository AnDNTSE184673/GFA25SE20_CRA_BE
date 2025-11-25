using Microsoft.AspNetCore.Http;
using Repository.DTO.RequestDTO.CarRegister;
using Repository.DTO.RequestDTO.DriverLicense;
using Repository.DTO.ResponseDTO.CarRegister;
using Repository.DTO.ResponseDTO.DriverLicense;
using Repository.DTO.ResponseDTO.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public interface IDriverLicenseService
    {
        Task<DriverLicenseView> UpdateDriverLicenseAsync(List<IFormFile> images, Guid userId);
        Task<(string status, ApproveLicenseView view)> ApproveLicenseAsync(LicenseSearchForm form, bool isApproved);
        Task<(string[] signedUrl, List<DriverLicenseView> view)> GetAllDocumentsAsync();
        Task<(string[] signedUrl, List<DriverLicenseView> view)> GetCarRegDocById(GetCarRegForm form);
    }
}
