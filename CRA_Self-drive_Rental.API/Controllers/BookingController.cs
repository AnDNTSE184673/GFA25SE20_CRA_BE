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
        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
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

        [HttpPost("CreateBooking")]
        public async Task<IActionResult> CreateBooking([FromBody]BookingCreateRequest request)
        {
            var booking = await _bookingService.CreateBooking(request);
            if (booking != null) return (IActionResult)booking;
            return BadRequest();
        }

        [HttpPatch("UpdateBooking")]
        public async Task<IActionResult> UpdateBooking([FromBody]BookingUpdateRequest request)
        {
            if(!ModelState.IsValid) return BadRequest();
            var booking = await _bookingService.ChangeStatus(request.BookingId, request.Status.ToString());
            if (booking != null) return Ok(booking);
            return BadRequest();
        }
    }
}
