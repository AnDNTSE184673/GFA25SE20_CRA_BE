using AutoMapper;
using Repository.Base;
using Repository.Constant;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO;
using Repository.DTO.ResponseDTO.Booking;
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
        private readonly IMapper _mapper;
        public BookingService(UnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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

        public async Task<BookingView?> CreateBooking(BookingCreateRequest request)
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
                var invoice = await _unitOfWork._invoiceRepo.CreateInvoice(newInvoice);
                var newBooking = new Booking
                {
                    Id = Guid.NewGuid(),
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow,
                    Status = ConstantEnum.Status.Pending.ToString(),
                    UserId = request.CustomerId,
                    CarId = request.CarId,
                    InvoiceId = invoice.Id
                };
                await _unitOfWork._bookingRepo.CreateAsync(newBooking);
                _unitOfWork.CommitTransaction();
                await _unitOfWork.SaveChangesAsync();
                var createdBooking = await _unitOfWork._bookingRepo.GetByIdAsync(newBooking.Id);
                var bookingView = _mapper.Map<BookingView>(createdBooking);
                return bookingView;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                if (ex is AggregateException aggEx)
                {
                    foreach (var inner in aggEx.InnerExceptions)
                    {
                        Console.WriteLine($"[Inner Exception] {inner.Message}");
                        if (inner.InnerException != null)
                            Console.WriteLine($"[Deep Inner] {inner.InnerException.Message}");
                    }
                }
                else if (ex.InnerException != null)
                {
                    Console.WriteLine($"[Inner Exception] {ex.InnerException.Message}");
                }

                throw;
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
