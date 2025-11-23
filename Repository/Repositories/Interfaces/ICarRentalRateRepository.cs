using Repository.Base;
using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories.Interfaces
{
    public interface ICarRentalRateRepository : IGenericRepository<CarRentalRate>
    {
        Task<CarRentalRate> GetRateByCarAsync(Guid carId);
        Task<(string status, CarRentalRate obj)> CreateRateAsync(CarRentalRate carRate);
        Task<string> DeleteRateAsync(Guid carId);
        Task<CarRentalRate> UpdateRateAsync(CarRentalRate carRate);
    }
}
