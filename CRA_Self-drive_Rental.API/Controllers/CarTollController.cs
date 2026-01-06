using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.DTO.RequestDTO.CarToll;
using Service.Services.Implementation;

namespace CRA_Self_drive_Rental.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarTollController : ControllerBase
    {
        private readonly ICarTollService _carTollService;
        public CarTollController(ICarTollService carTollService)
        {
            _carTollService = carTollService;
        }

        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _carTollService.GetAllCarTolls();
            return Ok(result);
        }

        [HttpGet("/Booking/")]
        public async Task<IActionResult> GetByBookingId([FromQuery] Guid bookingId)
        {
            if (bookingId == Guid.Empty) return BadRequest();
            var result = await _carTollService.GetCarTollDetailsByBookingId(bookingId);
            if (result == null) return BadRequest();
            return Ok(result);
        }

        [HttpGet("/Booking/Num")]
        public async Task<IActionResult> GetByBookingNum([FromQuery] string bookingNum)
        {
            if (string.IsNullOrEmpty(bookingNum)) return BadRequest();
            var result = await _carTollService.GetCarTollByBookingNum(bookingNum);
            if (result == null) return BadRequest();
            return Ok(result);
        }

        [HttpGet("/Car/")]
        public async Task<IActionResult> GetByCarId([FromQuery] Guid carId)
        {
            if (carId == Guid.Empty) return BadRequest();
            var result = await _carTollService.GetCarTollsByCarId(carId);
            if (result == null) return BadRequest();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCarToll([FromForm] CarTollCreateRequest request)
        {
            if ((request.BookingId == Guid.Empty || request.CarId == Guid.Empty || request.Amount <= 0) return BadRequest();
            var result = await _carTollService.CreateNewCarToll(request);
            if (result == null) return BadRequest();
            return Ok(result);
        }

        [HttpPost("/NewToll")]
        public async Task<IActionResult> InsertToCarToll([FromForm] CarTollTransacRequest request)
        {
            if (request.BookingId == Guid.Empty || request.CarId == Guid.Empty || request.Amount <= 0) return BadRequest();
            var result = await _carTollService.InsertToCarToll(request);
            if (result == null) return BadRequest();
            return Ok(result);
        }

    }
}
