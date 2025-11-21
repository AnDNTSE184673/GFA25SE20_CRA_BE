using Repository.Data.Entities;
using Repository.DTO.RequestDTO.CarRentalRate;
using Repository.DTO.ResponseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public interface ICarRentalService
    {
        Task<(string status, CarRentalRateView view)> SetRentalRate(CarRentalRateForm form);
    }
}
