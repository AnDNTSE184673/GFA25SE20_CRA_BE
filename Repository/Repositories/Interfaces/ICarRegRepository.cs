using Microsoft.AspNetCore.Http;
using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories.Interfaces
{
    public interface ICarRegRepository
    {
        Task<List<CarRegistration>> GetCarRegsAsync();
        Task<(string status, CarRegistration regData)> UploadCarRegistration(CarRegistration data);
        Task<CarRegistration> FindCarRegById(Guid? carId, Guid? userId);
        Task<CarRegistration> FindCarRegByPath(string? filePath, string? bucket);
        Task<CarRegistration> UpdateCarReg(CarRegistration reg);
    }
}
