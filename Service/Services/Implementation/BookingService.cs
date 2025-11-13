using Repository.Base;
using Repository.Constant;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Repository.Constant.ConstantEnum;

namespace Service.Services.Implementation
{
    public class BookingService : IBookingService
    {
        private readonly UnitOfWork _unitOfWork;
        public BookingService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<Booking?> ChangeStatus(Guid bookingId, string status)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var booking = await _unitOfWork._bookingRepo.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    throw new Exception("Booking not found");
                }
                if (!Enum.TryParse<Status>(status, true, out var parsedStatus))
                {
                    throw new Exception($"Invalid status: {status}");
                };
                booking.Status = parsedStatus.ToString();
                _unitOfWork._bookingRepo.Update(booking);                
                await _unitOfWork.SaveChangesAsync();
                _unitOfWork.CommitTransaction();
                var updatedBooking = await _unitOfWork._bookingRepo.GetByIdAsync(bookingId);
                return updatedBooking;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception(ex.Message);
            }
        }

        public Task<Booking?> CreateBooking(BookingCreateRequest request)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var newInvoice = new InvoiceCreateRequest
                {
                    CustomerId = request.CustomerId,
                    VendorId = Guid.NewGuid(), // This should be set appropriately
                    CarId = request.CarId,
                    CarRate = request.carRentPrice,
                    Fees = request.bookingFee,
                    RentTime = request.rentime,
                    InvoiceDue = request.rentDateEnd,
                    RentType = request.rentType
                };
                var invoice = _unitOfWork._invoiceRepo.CreateInvoice(newInvoice).Result;
                var newBooking = new Booking
                {
                    Id = Guid.NewGuid(),
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow,
                    Status = request.status.ToString(),
                    UserId = request.CustomerId,
                    CarId = request.CarId,
                    InvoiceId = invoice.Id
                };
                _unitOfWork._bookingRepo.CreateAsync(newBooking).Wait();
                _unitOfWork.CommitTransaction();
                _unitOfWork.SaveChangesAsync().Wait();
                var createdBooking = _unitOfWork._bookingRepo.GetByIdAsync(newBooking.Id).Result;
                return Task.FromResult<Booking?>(createdBooking);
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Booking>> GetAllBooking()
        {
            return (List<Booking>)await _unitOfWork._bookingRepo.GetAllAsync();
        }

        public async Task<Booking> GetBooking(Guid id)
        {
            var booking = await _unitOfWork._bookingRepo.GetByIdAsync(id);
            if (booking == null)
            {
                throw new Exception("Booking not found");
            }
            return booking;
        }

        public async Task<List<Booking>?> GetBookingsFromCar(Guid carId)
        {
            var bookings = await  _unitOfWork._bookingRepo.GetBookingsFromCar(carId);
            if (bookings == null || bookings.Count == 0)
            {
                return null;
            }
            return bookings;
        }

        public async Task<List<Booking>?> GetBookingsFromCustomer(Guid customerId)
        {
            var bookings = await  _unitOfWork._bookingRepo.GetBookingsFromCustomer(customerId);
            if (bookings == null || bookings.Count == 0)
            {
                return null;
            }
            return bookings;
        }
    }
}
