using AutoMapper;
using Repository.Base;
using Repository.Constant;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO;
using Repository.DTO.RequestDTO.Schedule;
using Repository.DTO.ResponseDTO.Booking;
using Repository.DTO.ResponseDTO.Schedule;
using Repository.Extension;
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
        private readonly ITrackAsiaService _trackAsiaService;

        public BookingService(UnitOfWork unitOfWork, IMapper mapper, IScheduleService scheduleServ, ITrackAsiaService trackAsiaService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _scheduleServ = scheduleServ;
            _trackAsiaService = trackAsiaService;
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
                if (ConstantEnum.Statuses.CANCELLED.ToLower().Equals(status.ToLower()))
                {
                    var invoice = await _unitOfWork._invoiceRepo.GetByIdAsync(booking.InvoiceId);
                    if (invoice == null)
                    {
                        throw new Exception("Invoice not found for the booking");
                    }
                    invoice.Status = ConstantEnum.Status.Cancelled.ToString();
                    var payments = await _unitOfWork._paymentRepo.GetPaymentsByInvoiceId(invoice.Id);
                    foreach (var payment in payments)
                    {
                        payment.Status = ConstantEnum.Status.Cancelled.ToString();
                        payment.UpdateDate = DateTime.UtcNow;
                        _unitOfWork._paymentRepo.Update(payment);
                    }
                    _unitOfWork._invoiceRepo.Update(invoice);
                    _unitOfWork.SaveChanges();

                }
                if (ConstantEnum.Statuses.CONFIRMED.ToLower().Equals(status.ToLower()))
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
                        payment.Status = ConstantEnum.Status.Success.ToString();
                        payment.UpdateDate = DateTime.UtcNow;
                        payment.PaymentMethod = "Cash on Delivery";
                        _unitOfWork._paymentRepo.Update(payment);
                    }
                    var existCar = await _unitOfWork._carRepo.GetByIdAsync(booking.CarId);
                    existCar.Status = ConstantEnum.Statuses.RESERVED;
                    await _unitOfWork._carRepo.UpdateCarAsync(existCar);
                    _unitOfWork.SaveChanges();
                }
                booking.Status = status;
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

        public async Task<(BookingView? booking, ScheduleView schedule)> CreateBooking(BookingCreateRequest request)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var holidayChecker = new HolidayChecker();
                var isHoliday = holidayChecker.IsHolidayInDateRange(request.PickupTime, request.DropoffTime);
                var ca = await _unitOfWork._carRepo.GetByIdAsync(request.CarId);
                var parkLot = await _unitOfWork._lotRepo.GetLotByNameAsync(request.PickupPlace);
                var carParkLot = await _unitOfWork._lotRepo.GetByIdAsync(ca.PrefLotId);
                int distance;
                if (parkLot != null)
                {
                    distance = 0;
                }
                else
                {
                    distance = (int)await _trackAsiaService.GetDistanceBetween(request.PickupPlace, carParkLot.Address);
                    distance += (int)await _trackAsiaService.GetDistanceBetween(carParkLot.Address, request.DropoffPlace);
                }
                    var newInvoice = new InvoiceCreateRequest
                    {
                        CustomerId = request.CustomerId,
                        VendorId = Guid.NewGuid(), // This should be set appropriately
                        CarId = request.CarId,
                        CarRate = request.carRentPrice,
                        Fees = (decimal)request.bookingFee,
                        RentTime = request.rentime,
                        InvoiceDue = request.DropoffTime,
                        RentType = request.rentType,
                        IsHoliday = isHoliday,
                        DistanceInM = distance
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
                existCar.Status = ConstantEnum.Statuses.RESERVED;
                await _unitOfWork._carRepo.UpdateCarAsync(existCar);

                var createdBooking = await _unitOfWork._bookingRepo.GetByIdAsync(newBooking.Id);
                var bookingView = _mapper.Map<BookingView>(createdBooking);
                await _unitOfWork.SaveChangesAsync();
                _unitOfWork.CommitTransaction();                
                return (bookingView, _mapper.Map<ScheduleView>(createdSchedule.view));
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

        public async Task<BookingView?> GetBookingFromInvoice(Guid invoiceId)
        {
            var booking = await _unitOfWork._bookingRepo.GetLatestBookingFromInvoice(invoiceId);
            if (booking == null)
            {
                return null;
            }
            return _mapper.Map<BookingView>(booking);
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
