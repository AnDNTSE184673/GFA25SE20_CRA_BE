using AutoMapper;
using Repository.Base;
using Repository.Constant;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO;
using Repository.DTO.RequestDTO.Schedule;
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
        private readonly IScheduleService _scheduleServ;

        public BookingService(UnitOfWork unitOfWork, IMapper mapper, IScheduleService scheduleServ)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _scheduleServ = scheduleServ;
        }

        public async Task<BookingView?> ChangeStatus(Guid bookingId, string status)
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
                if (parsedStatus == Status.Cancelled)
                {
                    var invoice = await _unitOfWork._invoiceRepo.GetByIdAsync(booking.InvoiceId);
                    if (invoice == null)
                    {
                        throw new Exception("Invoice not found for the booking");
                    }
                    invoice.Status = ConstantEnum.Status.Refunded.ToString();
                    var payments = await _unitOfWork._paymentRepo.GetPaymentsByInvoiceId(invoice.Id);
                    foreach (var payment in payments)
                    {
                        payment.Status = ConstantEnum.Status.Refunded.ToString();
                        _unitOfWork._paymentRepo.Update(payment);
                        _unitOfWork._paymentRepo.Update(payment);
                    }
                    _unitOfWork._invoiceRepo.Update(invoice);                    

                }
                if (parsedStatus == Status.Completed)
                {
                    var invoice = await _unitOfWork._invoiceRepo.GetByIdAsync(booking.InvoiceId);
                    if (invoice == null)
                    {
                        throw new Exception("Invoice not found for the booking");
                    }
                    invoice.Status = ConstantEnum.Status.Completed.ToString();
                    _unitOfWork._invoiceRepo.Update(invoice);
                    var payments = await _unitOfWork._paymentRepo.GetPaymentsByInvoiceId(invoice.Id);
                    foreach (var payment in payments)
                    {
                        payment.Status = ConstantEnum.Status.SUCCESS.ToString();
                        _unitOfWork._paymentRepo.Update(payment);
                    }
                }
                booking.Status = parsedStatus.ToString();
                _unitOfWork._bookingRepo.Update(booking);                
                await _unitOfWork.SaveChangesAsync();
                _unitOfWork.CommitTransaction();
                var updatedBooking = await _unitOfWork._bookingRepo.GetByIdAsync(bookingId);
                return _mapper.Map<BookingView>(updatedBooking);
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
                    InvoiceDue = request.DropoffTime,
                    RentType = request.rentType
                };
                var invoice = await _unitOfWork._invoiceRepo.CreateInvoice(newInvoice);
                var newBooking = new Booking
                {
                    Id = Guid.NewGuid(),
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow,
                    PickupPlace = request.PickupPlace,
                    PickupTime = request.PickupTime,
                    DropoffPlace = request.DropoffPlace,
                    DropoffTime = request.DropoffTime,
                    Status = ConstantEnum.Status.Pending.ToString(),
                    UserId = request.CustomerId,
                    CarId = request.CarId,
                    InvoiceId = invoice.Id
                };
                await _unitOfWork._bookingRepo.CreateAsync(newBooking);
                await _unitOfWork._paymentRepo.CreateNewPaymentForBookingFee(newBooking.InvoiceId);
                await _unitOfWork._paymentRepo.CreateNewPaymentForRentalFee(newBooking.InvoiceId);

                var bookingSchedule = new CreateScheduleForm
                {
                    Title = ConstantEnum.ScheduleDefaultTitle.PICKUP,
                    Location = request.PickupPlace,
                    StartDate = request.PickupTime,
                    EndDate = request.PickupTime,
                    ScheduleType = ConstantEnum.ScheduleTypeConstants.Pickup,
                    Priority = 1,
                    Note = "",
                    IsBlocking = true, //Car is not available for rent atm
                    CarId = request.CarId,
                    UserId = request.CustomerId,
                    BookingId = newBooking.Id
                };

                var createdSchedule = await _scheduleServ.SetCarSchedulesInnerServiceAsync(bookingSchedule);

                var existCar = await _unitOfWork._carRepo.GetByIdAsync(request.CarId);
                existCar.Status = ConstantEnum.Statuses.INACTIVE;
                await _unitOfWork._carRepo.UpdateCarAsync(existCar);

                var createdBooking = await _unitOfWork._bookingRepo.GetByIdAsync(newBooking.Id);
                var bookingView = _mapper.Map<BookingView>(createdBooking);
                await _unitOfWork.SaveChangesAsync();
                _unitOfWork.CommitTransaction();                
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
