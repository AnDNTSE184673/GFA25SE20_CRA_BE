using Microsoft.AspNetCore.Mvc;
using Repository.Constant;
using Repository.DTO.RequestDTO.ParkingLot;
using Service.Services;

namespace CRA_Self_drive_Rental.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParkLotController : ControllerBase
    {
        private readonly IParkingLotService _lotServ;

        public ParkLotController(IParkingLotService lotServ)
        {
            _lotServ = lotServ;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLots() // TODO: FILTER SEARCH
        {
            try
            {
                var result = await _lotServ.GetAllLotAsync();
                return result.Count <= 0
                    ? StatusCode(404, new
                    {
                        Message = "No data found, check log"
                    })
                    : Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RegisterLot(PostParkingLotForm form) // TODO: FILTER SEARCH
        {
            try
            {
                var result = await _lotServ.RegisterLotAsync(form);
                return result.status.Contains(ConstantEnum.RepoStatus.FAILURE)
                    ? StatusCode(500, new
                    {
                        Message = "Data creation error, check log and form"
                    })
                    : Ok(result.lot);
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
