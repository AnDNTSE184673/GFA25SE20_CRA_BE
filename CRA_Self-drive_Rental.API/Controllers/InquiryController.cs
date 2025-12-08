using Microsoft.AspNetCore.Mvc;
using Repository.Constant;
using Repository.DTO.RequestDTO.Inquiry;
using Repository.Extension.SupabaseFileUploader;
using Service.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CRA_Self_drive_Rental.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InquiryController : ControllerBase
    {
        private readonly IInquiryService _inquiryServ;

        public InquiryController(IInquiryService inquiryServ)
        {
            _inquiryServ = inquiryServ;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetInquiryByUser(Guid userId)
        {
            try
            {
                var result = await _inquiryServ.GetAllUniqueInquiriesByUser(userId);
                return result.Count <= 0
                    ? StatusCode(StatusCodes.Status404NotFound, new
                    {
                        Message = "There are no Inquiry yet!"
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

        [HttpGet("chatLog")]
        public async Task<IActionResult> GetAllInquiryOfTwoSides(Guid senderId, Guid receiverId)
        {
            try
            {
                var result = await _inquiryServ.GetInquiryTreeByBothSides(senderId, receiverId);
                return result.Count <= 0
                    ? StatusCode(StatusCodes.Status404NotFound, new
                    {
                        Message = "There are no Inquiry yet!"
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

        [HttpPost("initial")]
        public async Task<IActionResult> StartInquiry([FromForm] CreateInquiryForm form)
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

                var result = await _inquiryServ.LeaveCarInquiry(form);
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

        [HttpPost("answer")]
        public async Task<IActionResult> AnswerInquiry([FromForm] AnswerInquiryForm form)
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

                var result = await _inquiryServ.AnswerCarInquiry(form);
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
        public async Task<IActionResult> UpdateInquiry(Guid id, [FromForm] EditInquiryForm form)
        {
            try
            {
                var result = await _inquiryServ.EditCarInquiry(id, form);
                return result != null
                    ? StatusCode(StatusCodes.Status404NotFound, new
                    {
                        Message = "Inquiry doesn't exist!"
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
        public async Task<IActionResult> DeleteInquiry(Guid id)
        {
            try
            {
                var result = await _inquiryServ.DeleteCarInquiry(id);
                return result.Equals(ConstantEnum.RepoStatus.FAILURE)
                    ? StatusCode(StatusCodes.Status404NotFound, new
                    {
                        Message = "Inquiry doesn't exist!"
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
