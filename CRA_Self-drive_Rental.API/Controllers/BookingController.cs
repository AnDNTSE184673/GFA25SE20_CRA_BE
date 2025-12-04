using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.DTO.RequestDTO;
using Service.Services;

namespace CRA_Self_drive_Rental.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IPaymentService _paymentService;
        public BookingController(IBookingService bookingService, IPaymentService paymentService)
        {
            _bookingService = bookingService;
            _paymentService = paymentService;
        }

        [HttpGet("GetAllBookings")]
        public async Task<IActionResult> GetAllBookings()
        {
            var bookings = await _bookingService.GetAllBooking();
            return Ok(bookings);
        }

        [HttpGet("GetBookingById/{bookingId}")]
        public async Task<IActionResult> GetBookingById(Guid bookingId)
        {
            var booking = await _bookingService.GetBooking(bookingId);
            if (booking != null)
            {
                return Ok(booking);
            }
            return NotFound();
        }

        [HttpGet("GetBookingsFromCustomer/{cusId}")]
        public async Task<IActionResult> GetBookingFromCustomer(Guid cusId)
        {
            var bookings = await _bookingService.GetBookingsFromCustomer(cusId);
            if (bookings != null)
            {
                return Ok(bookings);
            }
            return NotFound();
        }

        [HttpGet("GetBookingsForCar/{carId}")]
        public async Task<IActionResult> GetBookingForCar(Guid carId)
        {
            var bookings = await _bookingService.GetBookingsFromCar(carId);
            if(bookings != null) return Ok(bookings);
            return NotFound();
        }

        [HttpGet("GetBookingsByInvoice/{invoiceId}")]
        public async Task<IActionResult> GetBookingsByInvoice(Guid invoiceId)
        {
            if (invoiceId == Guid.Empty) return BadRequest();
            var bookings = await _bookingService.GetBookingFromInvoice(invoiceId);
            if (bookings != null)
            {
                return Ok(bookings);
            }
            return NotFound();
        }

        [HttpGet("GetBookingsByBookNum/{bookingNum}")]
        public async Task<IActionResult> GetBookingsByBookNum(string bookingNum)
        {
            if (string.IsNullOrEmpty(bookingNum)) return BadRequest();
            var bookings = await _bookingService.GetBookingFromBookingNumber(bookingNum);
            if (bookings != null)
            {
                return Ok(bookings);
            }
            return NotFound();
        }

        [HttpPost("CreateBooking")]
        public async Task<IActionResult> CreateBooking([FromBody]BookingCreateRequest request)
        {
            if(!ModelState.IsValid) return BadRequest();
            var booking = await _bookingService.CreateBooking(request);
            var payment = await _paymentService.CreatePayOSFromBooking(booking.booking.Id);
            var obj = new { Payment = payment.Item2, Booking = booking, Schedule = booking.schedule, };
            if (booking.booking != null) return Ok(obj);
            return BadRequest();
        }

        [HttpPatch("UpdateBooking")]
        public async Task<IActionResult> UpdateBooking([FromBody]BookingUpdateRequest request)
        {
            if(!ModelState.IsValid) return BadRequest();
            var booking = await _bookingService.ChangeStatus(request.BookingId, request.Status);
            if (booking != null) return Ok(booking);
            return BadRequest();
        }
    }
}
