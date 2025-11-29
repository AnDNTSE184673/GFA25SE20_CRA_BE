using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.DTO.RequestDTO.Map;
using Service.Services;

namespace CRA_Self_drive_Rental.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrackAsiaController : ControllerBase
    {
        private readonly ITrackAsiaService _trackAsiaService;
        public TrackAsiaController(ITrackAsiaService trackAsiaService)
        {
            _trackAsiaService = trackAsiaService;
        }

        [HttpPost("GetReverseGeocoding")]
        public async Task<IActionResult> GetReverseGeocoding([FromBody] ReverseGeo request)
        {
            try
            {
                var addresses = await _trackAsiaService.GetAddressFromCoordinate(request.Latitude, request.Longitude);
                if (addresses != null)
                {
                    return Ok(new
                    {
                        FormattedAddress = addresses.Value.Item1,
                        OldFormattedAddress = addresses.Value.Item2
                    });
                }
                return NotFound("Coordinates not found for the specified location.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = ex.Message
                });
            }
        }
    }
}
