using Repository.Base;
using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories.Interfaces
{
    public interface IBookingRepository : IGenericRepository<Booking>
    {
        Task<Booking?> ChangeStatus(Guid bookingId, string status);

        Task<List<Booking>?> GetBookingsFromCustomer(Guid customerId);
        Task<List<Booking>?> GetBookingsFromCar(Guid carId);
        
    }
}
