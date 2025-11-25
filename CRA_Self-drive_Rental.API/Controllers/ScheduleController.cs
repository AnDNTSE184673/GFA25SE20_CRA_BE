using Microsoft.AspNetCore.Mvc;
using Repository.Constant;
using Repository.DTO.RequestDTO.Schedule;
using Service.Services;

namespace CRA_Self_drive_Rental.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _scheduleServ;

        public ScheduleController(IScheduleService scheduleService)
        {
            _scheduleServ = scheduleService;
        }

        [HttpGet("car")]
        public async Task<IActionResult> GetScheduleByCar(Guid carId)
        {
            try
            {
                var result = await _scheduleServ.GetAllSchedulesOfCarAsync(carId);
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

        [HttpGet("booking")]
        public async Task<IActionResult> GetScheduleByBooking(Guid bookingId)
        {
            try
            {
                var result = await _scheduleServ.GetAllSchedulesOfBookingAsync(bookingId);
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

        [HttpGet("user")]
        public async Task<IActionResult> GetScheduleByUser(Guid userId)
        {
            try
            {
                var result = await _scheduleServ.GetAllSchedulesOfUserAsync(userId);
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

        [HttpPost]
        public async Task<IActionResult> CreateSchedule(CreateScheduleForm form)
        {
            try
            {
                var result = await _scheduleServ.SetCarSchedulesAsync(form);
                return result.status.Equals(ConstantEnum.RepoStatus.FAILURE)
                    ? StatusCode(StatusCodes.Status400BadRequest, new
                    {
                        Message = "Data creation error, check log and form"
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

        [HttpPost("checkIn")]
        public async Task<IActionResult> CheckInSchedule(Guid userId, Guid carId)
        {
            try
            {
                var result = await _scheduleServ.CheckInAsync(userId,carId);
                return result.status.Equals(ConstantEnum.RepoStatus.FAILURE)
                    ? StatusCode(StatusCodes.Status400BadRequest, new
                    {
                        Message = "Data creation error, check log and form"
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

        [HttpPost("checkOut")]
        public async Task<IActionResult> CheckOutSchedule(Guid userId, Guid carId)
        {
            try
            {
                var result = await _scheduleServ.CheckOutAsync(userId, carId);
                return result.status.Equals(ConstantEnum.RepoStatus.FAILURE)
                    ? StatusCode(StatusCodes.Status400BadRequest, new
                    {
                        Message = "Data creation error, check log and form"
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

        [HttpPost("maintenance")]
        public async Task<IActionResult> MaintenanceSchedule(MaintenanceSchedule form)
        {
            try
            {
                var result = await _scheduleServ.SetMaintenanceAsync(form);
                return result.status.Equals(ConstantEnum.RepoStatus.FAILURE)
                    ? StatusCode(StatusCodes.Status400BadRequest, new
                    {
                        Message = "Data creation error, check log and form"
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

        [HttpPatch("statusChange/{scheduleId}")]
        public async Task<IActionResult> MaintenanceSchedule(Guid bookingId, bool isCompleted)
        {
            try
            {
                var result = await _scheduleServ.StatusChangeAsync(bookingId, isCompleted, false);
                return result == null
                    ? StatusCode(StatusCodes.Status400BadRequest, new
                    {
                        Message = "Data creation error, check log and form"
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

        [HttpPatch]
        public async Task<IActionResult> UpdateSchedule(UpdateScheduleForm form)
        {
            try
            {
                var result = await _scheduleServ.UpdateCarSchedulesAsync(form);
                return result == null
                    ? StatusCode(StatusCodes.Status400BadRequest, new
                    {
                        Message = "Data creation error, check log and form"
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

        [HttpDelete]
        public async Task<IActionResult> DeleteSchedule(Guid scheduleId)
        {
            try
            {
                var result = await _scheduleServ.RemoveSchedulesAsync(scheduleId);
                return result == null
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
