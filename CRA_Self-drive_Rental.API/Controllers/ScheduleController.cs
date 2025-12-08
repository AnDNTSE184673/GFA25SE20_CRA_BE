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

        [HttpGet("car/{carId}")]
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

        [HttpGet("booking/{bookingId}")]
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

        [HttpGet("user/{userId}")]
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
        public async Task<IActionResult> CheckInSchedule([FromForm] CICOForm form)
        {
            try
            {
                var userAgent = Request.Headers["User-Agent"].ToString();
                var result = await _scheduleServ.CheckInAsync(form, userAgent);
                return result.status.Equals(ConstantEnum.RepoStatus.FAILURE)
                    ? StatusCode(StatusCodes.Status400BadRequest, new
                    {
                        Message = "Data creation error, check log and form"
                    })
                    : Ok(new
                    {
                        NewSchedule = result.view,
                        Image = result.image
                    });
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
        public async Task<IActionResult> CheckOutSchedule([FromForm] CICOForm form)
        {
            try
            {
                var userAgent = Request.Headers["User-Agent"].ToString();
                var result = await _scheduleServ.CheckOutAsync(form, userAgent);
                return result.status.Equals(ConstantEnum.RepoStatus.FAILURE)
                    ? StatusCode(StatusCodes.Status400BadRequest, new
                    {
                        Message = "Data creation error, check log and form"
                    })
                    : Ok(new 
                    {
                        NewSchedule = result.view,
                        Image = result.image
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = ex.Message
                });
            }
        }

        [HttpGet("checkInOut/info")]
        public async Task<IActionResult> GetCICOImages([FromQuery] CICOImageSearch form)
        {
            try
            {
                var result = await _scheduleServ.GetCICOImageByBooking(form);
                return result.view == null
                    ? StatusCode(404, new
                    {
                        Message = "No data found, check log"
                    })
                    : Ok(new
                    {
                        //Urls = result.signedUrl,
                        View = result.view
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = ex.Message
                });
            }
        }

        [HttpPost("checkIn/images")]
        public async Task<IActionResult> UploadCheckInImages([FromForm]CheckInOutImages form)
        {
            try
            {
                var result = await _scheduleServ.UploadImageWhenCheckInOut(form);
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
        public async Task<IActionResult> StatusChange(Guid bookingId, bool isCompleted)
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
