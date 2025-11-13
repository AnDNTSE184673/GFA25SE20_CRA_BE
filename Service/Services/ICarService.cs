using Repository.Data.Entities;
using Repository.DTO.RequestDTO.Car;
using Repository.DTO.ResponseDTO.Car;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public interface ICarService
    {
        Task<(string status, CarView car)> RegisterCarAsync(CarInfoForm form);
    }
}
