using Microsoft.AspNetCore.Mvc;
using Repository.Constant;
using Repository.DTO.RequestDTO.Feedback;
using Repository.Extension.SupabaseFileUploader;
using Service.Services;
using System.Linq;

namespace CRA_Self_drive_Rental.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackServ;

        public FeedbackController(IFeedbackService feedbackServ)
        {
            _feedbackServ = feedbackServ;
        }

        [HttpGet("{carId}")]
        public async Task<IActionResult> GetFeedbackByCar(Guid carId)
        {
            try
            {
                var result = await _feedbackServ.GetCarFeedbacks(carId);
                return result.Count <= 0
                    ? StatusCode(StatusCodes.Status404NotFound, new
                    {
                        Message = "There are no feedback yet!"
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
        public async Task<IActionResult> CreateFeedback([FromForm] CreateFeedbackForm form)
        {
            try
            {
                var allowedExtensions = new HashSet<string>
                {
                    ".jpg", ".jpeg", ".png", ".gif", ".webp",
                    ".mp4", ".mov", ".avi", ".mkv"
                };

                string ext = "";

                foreach (var file in form.Medias)
                {
                    ext = Path.GetExtension(file.FileName).ToLowerInvariant();

                    if (!allowedExtensions.Contains(ext))
                        return BadRequest($"Unsupported file extension: {ext}");

                    var mime = MimeTypeHelper.GetMimeType(ext);

                    if (mime == "application/octet-stream")
                        return BadRequest("Unsupported file extension.");

                    if (!MimeTypeHelper.IsValidFile(file))
                        return BadRequest($"File signature doesn't match extension {ext}. This file may be unsafe.");
                }

                var result = await _feedbackServ.LeaveCarFeedback(form);
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

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateFeedback(Guid id, [FromForm] EditFeedbackForm form)
        {
            try
            {
                var result = await _feedbackServ.EditCarFeedback(id, form);
                return result != null
                    ? StatusCode(StatusCodes.Status404NotFound, new
                    {
                        Message = "Feedback doesn't exist!"
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
        public async Task<IActionResult> DeleteFeedback(Guid id)
        {
            try
            {
                var result = await _feedbackServ.DeleteCarFeedback(id);
                return result.Equals(ConstantEnum.RepoStatus.FAILURE)
                    ? StatusCode(StatusCodes.Status404NotFound, new
                    {
                        Message = "Feedback doesn't exist!"
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
