using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Services;

namespace CRA_Self_drive_Rental.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditController : ControllerBase
    {
        private readonly IStaffLogService _logServ;
        private readonly ICarHandoverService _handoverServ;

        public AuditController(IStaffLogService logServ, ICarHandoverService handoverServ)
        {
            _logServ = logServ;
            _handoverServ = handoverServ;
        }

        [HttpGet("staffLogs")]
        public async Task<IActionResult> GetStaffLogsAsync()
        {
            try
            {
                var result = await _logServ.GetStaffLogsAsync();
                return !result.Any()
                    ? StatusCode(StatusCodes.Status404NotFound, new
                    {
                        Message = "Data fetch error, check log and form"
                    })
                    : Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = ex.Message
                });
            }
        }

        [HttpGet("staffLogs/{staffid}")]
        public async Task<IActionResult> GetStaffLogsByStaffAsync(Guid staffId)
        {
            try
            {
                var result = await _logServ.GetStaffLogsByStaffAsync(staffId);
                return !result.Any()
                    ? StatusCode(StatusCodes.Status404NotFound, new
                    {
                        Message = "Data fetch error, check log and form"
                    })
                    : Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = ex.Message
                });
            }
        }

        [HttpGet("carHandover")]
        public async Task<IActionResult> GetCarHandoversAsync()
        {
            try
            {
                var result = await _handoverServ.GetCarHandoversAsync();
                return !result.Any()
                    ? StatusCode(StatusCodes.Status404NotFound, new
                    {
                        Message = "Data fetch error, check log and form"
                    })
                    : Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = ex.Message
                });
            }
        }

        [HttpGet("carHandover/{scheduleId}")]
        public async Task<IActionResult> GetCarHandoversByScheduleAsync(Guid scheduleId)
        {
            try
            {
                var result = await _handoverServ.GetCarHandoversBySchedulesAsync(scheduleId);
                return !result.Any()
                    ? StatusCode(StatusCodes.Status404NotFound, new
                    {
                        Message = "Data fetch error, check log and form"
                    })
                    : Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = ex.Message
                });
            }
        }
    }
}
