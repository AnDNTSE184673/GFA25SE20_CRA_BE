using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.DTO.RequestDTO.DriverLicense;
using Service.Services;

namespace CRA_Self_drive_Rental.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FPTAIController : ControllerBase
    {
        private readonly IFPTAIService _fptaiService;
        public FPTAIController(IFPTAIService fptaiService)
        {
            _fptaiService = fptaiService;
        }

        [HttpPost("ExtractDriverLicenseInfo")]
        public async Task<IActionResult> ExtractDriverLicenseInfo([FromForm] ExtractDriverLicenseRequest request)
        {
            if (request.Image == null || request.Image.Length == 0)
            {
                return BadRequest("No image file provided.");
            }
            try
            {
                var drvLicenseInfo = await _fptaiService.ExtractDriverLicenseInfo(request.Image);
                return Ok(drvLicenseInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
