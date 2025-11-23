using Microsoft.AspNetCore.Http;
using Repository.Base;
using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories.Interfaces
{
    public interface ICarRegRepository : IGenericRepository<CarRegistration>
    {
        Task<List<CarRegistration>> GetCarRegsAsync();
        Task<(string status, CarRegistration regData)> UploadCarRegistration(CarRegistration data);
        Task AddCarRegistration(CarRegistration data);
        Task<List<CarRegistration>> FindCarRegById(Guid carId, Guid userId);
        Task<List<CarRegistration>> FindCarRegByPath(string filePath, string bucket);
        Task<CarRegistration> UpdateCarReg(CarRegistration reg);
        Task<List<CarRegistration>> FindCarRegByInfo(string licensePlate, string email);
    }
}
