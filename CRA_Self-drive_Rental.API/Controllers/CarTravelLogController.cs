using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    }
}
