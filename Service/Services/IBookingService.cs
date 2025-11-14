using Repository.Data.Entities;
using Repository.DTO.RequestDTO;
using Repository.DTO.ResponseDTO.Booking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public interface IBookingService
    {
        Task<List<Booking>> GetAllBooking();
        Task<Booking> GetBooking(Guid id);
        Task<List<Booking>?> GetBookingsFromCustomer(Guid customerId);
        Task<List<Booking>?> GetBookingsFromCar(Guid carId);
        Task<BookingView?> CreateBooking(BookingCreateRequest request);
        Task<Booking?> ChangeStatus(Guid bookingId, string status);
    }
}
