using Microsoft.EntityFrameworkCore;
using Repository.Base;
using Repository.Data;
using Repository.Data.Entities;
using Repository.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class BookingRepository : GenericRepository<Booking>, IBookingRepository
    {
        private readonly CRA_DbContext _context;

        public BookingRepository(CRA_DbContext context) : base(context)
        {
            _context = context;
        }

        public Task<Booking?> ChangeStatus(Guid bookingId, string status)
        {
            var booking = _context.BookingHistories.FirstOrDefault(b => b.Id == bookingId);
            if (booking != null)
            {
                booking.Status = status;
                _context.SaveChanges();
            }
            var updatedBooking = _context.BookingHistories.FirstOrDefault(b => b.Id == bookingId);
            return Task.FromResult(updatedBooking);
        }

        public async Task<List<Booking>?> GetBookingsFromCar(Guid carId)
        {
            var bookings = await  _context.BookingHistories
                .Where(b => b.CarId == carId)
                .ToListAsync();
            return bookings;
        }

        public async Task<List<Booking>?> GetBookingsFromCustomer(Guid customerId)
        {
            var bookings = await _context.BookingHistories
                .Where(b => b.UserId == customerId)
                .ToListAsync();
            return bookings;
        }
    }
}
