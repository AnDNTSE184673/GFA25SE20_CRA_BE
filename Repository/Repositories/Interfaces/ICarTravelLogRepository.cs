using Repository.Base;
using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories.Interfaces
{
    public interface ICarTravelLogRepository : IGenericRepository<CarTravelLog>
    {
        Task<List<CarTravelLog>> GetByCarIdAsync(Guid carId);
        Task<List<CarTravelLog>> GetByBookingIdAsync(Guid bookingId);
        Task<List<CarTravelLog>> GetByCarAndBookingAsync(Guid carId, Guid bookingId);
    }
}
