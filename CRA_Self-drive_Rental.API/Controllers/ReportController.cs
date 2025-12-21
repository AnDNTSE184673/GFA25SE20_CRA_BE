using Microsoft.AspNetCore.Mvc;
using Repository.Constant;
using Repository.DTO.RequestDTO.Report;
using Repository.Extension.SupabaseFileUploader;
using Service.Services;

namespace CRA_Self_drive_Rental.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportServ;

        public ReportController(IReportService reportServ)
        {
            _reportServ = reportServ;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllReports()
        {
            try
            {
                var result = await _reportServ.GetAllReports();
                return result.Count <= 0
                    ? StatusCode(StatusCodes.Status404NotFound, new
                    {
                        Message = "There are no report yet!"
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

        [HttpGet("reportedCar/{carId}")]
        public async Task<IActionResult> GetReportsByCar(Guid carId)
        {
            try
            {
                var result = await _reportServ.GetCarReports(carId);
                return result.Count <= 0
                    ? StatusCode(StatusCodes.Status404NotFound, new
                    {
                        Message = "There are no report yet!"
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

        [HttpGet("reportedUser/{userId}")]
        public async Task<IActionResult> GetReportsByReportedUser(Guid userId)
        {
            try
            {
                var result = await _reportServ.GetReportsByReportedUser(userId);
                return result.Count <= 0
                    ? StatusCode(StatusCodes.Status404NotFound, new
                    {
                        Message = "There are no report yet!"
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
        public async Task<IActionResult> GetReportsByUser(Guid userId)
        {
            try
            {
                var result = await _reportServ.GetReportsByUser(userId);
                return result.Count <= 0
                    ? StatusCode(StatusCodes.Status404NotFound, new
                    {
                        Message = "There are no report yet!"
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

        [HttpPost("reportedCar")]
        public async Task<IActionResult> CreateCarReport([FromForm] CarReportForm form)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest("Report form is not valid, check input");
                var result = await _reportServ.CreateCarReport(form);
                return result.status.Contains(ConstantEnum.RepoStatus.FAILURE)
                    ? StatusCode(500, new
                    {
                        Message = "Data creation error, check log and form"
                    })
                    : Ok(result.view);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = ex.Message
                });
            }
        }

        [HttpPost("reportedUser")]
        public async Task<IActionResult> CreateUserReport([FromForm] UserReportForm form)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest("Report form is not valid, check input");
                var result = await _reportServ.CreateUserReport(form);
                return result.status.Contains(ConstantEnum.RepoStatus.FAILURE)
                    ? StatusCode(500, new
                    {
                        Message = "Data creation error, check log and form"
                    })
                    : Ok(result.view);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = ex.Message
                });
            }
        }

        [HttpPatch("approve")]
        public async Task<IActionResult> UpdateReport([FromBody] ApproveReportForm form)
        {
            try
            {
                var result = await _reportServ.ApproveCarReport(form);
                return result != null
                    ? StatusCode(StatusCodes.Status404NotFound, new
                    {
                        Message = "Report doesn't exist!"
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

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateReport(Guid id, [FromForm] EditReportForm form)
        {
            try
            {
                var result = await _reportServ.EditCarReport(id, form);
                return result != null
                    ? StatusCode(StatusCodes.Status404NotFound, new
                    {
                        Message = "Report doesn't exist!"
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReport(Guid id)
        {
            try
            {
                var result = await _reportServ.DeleteCarReport(id);
                return result.Equals(ConstantEnum.RepoStatus.FAILURE)
                    ? StatusCode(StatusCodes.Status404NotFound, new
                    {
                        Message = "Report doesn't exist!"
                    })
                    : NoContent();
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
