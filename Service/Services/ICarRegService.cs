using Microsoft.AspNetCore.Http;
using Repository.DTO.RequestDTO.CarRegister;
using Repository.DTO.ResponseDTO.CarRegister;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public interface ICarRegService
    {
        Task<(string status, CarRegView view)> ApproveDocumentsAsync(CarRegForm form, bool isApproved);
        Task<List<CarRegView>> GetAllDocumentsAsync();
        Task<(string signedUrl, CarRegView view)> GetCarRegDocById(GetCarRegForm form);
        Task<(string signedUrl, CarRegView view)> GetCarRegDocByInfo(GetCarRegForm form);
        Task<(string signedUrl, CarRegView view)> GetCarRegDocByPath(GetCarRegForm form);
        Task<(string status, CarRegView regDoc)> SubmitRegisterDocument(IFormFile file, CarRegForm form);
    }
}
