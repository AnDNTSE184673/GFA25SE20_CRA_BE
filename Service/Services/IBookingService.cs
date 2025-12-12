using Repository.Data.Entities;
using Repository.DTO.RequestDTO;
using Repository.DTO.ResponseDTO.Booking;
using Repository.DTO.ResponseDTO.Schedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public interface IBookingService
    {
        Task<List<BookingView>> GetAllBooking();
        Task<BookingView> GetBooking(Guid id);
        Task<List<Booking>?> GetBookingsFromCustomer(Guid customerId);
        Task<List<Booking>?> GetBookingsFromCar(Guid carId);
        Task<BookingView?> GetBookingFromInvoice(Guid invoiceId);
        Task<BookingView?> GetBookingFromBookingNumber(string bookingNum);
        Task<(BookingView? booking, ScheduleView schedule)> CreateBooking(BookingCreateRequest request);
        Task<(BookingView? booking, ScheduleView? schedule)> ExtendBooking(BookingExtensionRequest request);
        Task<BookingView?> ChangeStatus(Guid bookingId, string status);
    }
}
