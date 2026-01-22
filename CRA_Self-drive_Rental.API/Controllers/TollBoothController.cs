using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Services;

namespace CRA_Self_drive_Rental.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TollBoothController : ControllerBase
    {
        private readonly ITollBoothService _tollService;
        public TollBoothController(ITollBoothService tollService)
        {
            _tollService = tollService;
        }

        [HttpGet("All")]
        public async Task<IActionResult> GetAllTollBooths()
        {
            var tollBooths = await _tollService.GetAllTollBoothsAsync();
            return Ok(tollBooths);
        }
    }
}
