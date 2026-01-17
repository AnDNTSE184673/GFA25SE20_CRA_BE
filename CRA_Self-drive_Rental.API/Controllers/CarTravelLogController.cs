using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.DTO.RequestDTO.CarTravelLog;
using Service.Services;

namespace CRA_Self_drive_Rental.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarTravelLogController : ControllerBase
    {
        private readonly ICarTravelLogService _carTravelLogService;
        public CarTravelLogController(ICarTravelLogService carTravelLogService)
        {
            _carTravelLogService = carTravelLogService;
        }

        [HttpGet("All")]
        public async Task<IActionResult> GetAllCarTravelLogs()
        {
            var result = await _carTravelLogService.GetAll();
            if (result == null || !result.Any())
            {
                return NotFound("No car travel logs found.");
            }
            return Ok(result);
        }

        [HttpGet("ByCar/{carId}")]
        public async Task<IActionResult> GetCarTravelLogsByCarId(Guid carId)
        {
            var result = await _carTravelLogService.GetByCarId(carId);
            if (result == null || !result.Any())
            {
                return NotFound($"No car travel logs found for Car ID: {carId}");
            }
            return Ok(result);
        }

        [HttpGet("ByBooking/{bookingId}")]
        public async Task<IActionResult> GetCarTravelLogsByBookingId(Guid bookingId)
        {
            var result = await _carTravelLogService.GetByBookingId(bookingId);
            if (result == null || !result.Any())
            {
                return NotFound($"No car travel logs found for Booking ID: {bookingId}");
            }
            return Ok(result);
        }

        [HttpGet("ByCarAndBooking")]
        public async Task<IActionResult> GetCarTravelLogsByCarAndBooking([FromQuery] Guid carId, [FromQuery] Guid bookingId)
        {
            var result = await _carTravelLogService.GetByCarAndBooking(carId, bookingId);
            if (result == null || !result.Any())
            {
                return NotFound($"No car travel logs found for Car ID: {carId} and Booking ID: {bookingId}");
            }
            return Ok(result);
        }

        [HttpPost("CreateRandom")]
        public async Task<IActionResult> CreateRandomCarTravelLogs([FromBody] CarTravelRandomCreate request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _carTravelLogService.CreateRandomLog(request.CarId, request.BookingId, request.NumOfToll);
            if (result == null || !result.Any())
            {
                return BadRequest("Failed to create random car travel logs.");
            }
            return Ok(result);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateCarTravelLog([FromBody] CarTravelCreate request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _carTravelLogService.CreateCarTravelLog(request.CarId, request.BookingId, request.TollBoothId);
            if (result == null || !result.Any())
            {
                return BadRequest("Failed to create car travel log.");
            }
            return Ok(result);
        }
    }
}
