using Microsoft.AspNetCore.Mvc;
using Repository.DTO.RequestDTO.Schedule;
using Service.Services;

namespace CRA_Self_drive_Rental.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;

        public ScheduleController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateSchedule(CreateScheduleForm form)
        {
            throw new NotImplementedException();
        }
    }
}
