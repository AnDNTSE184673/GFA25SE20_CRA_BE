using Microsoft.AspNetCore.Mvc;
using Repository.DTO.RequestDTO.Schedule;

namespace CRA_Self_drive_Rental.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleController : ControllerBase
    {
        [HttpPost("create")]
        public async Task<IActionResult> CreateSchedule(CreateScheduleForm form)
        {
            throw new NotImplementedException();
        }
    }
}
