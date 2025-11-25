using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.DTO.RequestDTO;
using Repository.DTO.RequestDTO.DriverLicense;
using Repository.DTO.RequestDTO.User;
using Service.Services;
using Swashbuckle.AspNetCore.Annotations;
using static System.Net.Mime.MediaTypeNames;

namespace CRA_Self_drive_Rental.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IDriverLicenseService _licenseService;

        public UserController(IUserService userService, IDriverLicenseService licenseService)
        {
            _userService = userService;
            _licenseService = licenseService;
        }

        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var response = await _userService.GetAllUsers();
            return Ok(response);
        }

        [HttpGet("GetUserById")]
        public async Task<IActionResult> GetUserById([FromQuery] Guid userId)
        {
            var response = await _userService.GetUserById(userId);
            if (response == null)
            {
                return NotFound("User not found.");
            }
            return Ok(response);
        }

        [HttpPatch("UpdateUserInfo")]
        public async Task<IActionResult> UpdateUserInfo([FromBody] UserUpdateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Invalid update data",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }
            var response = await _userService.UpdateUserInfo(request);
            return Ok(response);
        }

        [HttpPatch("upload-avatar/{userId}")]
        [SwaggerOperation(Summary = "Don't FromForm the IFormFile as it's already implied")]
        ///<summary>"Don't FromForm the IFormFile as it's already implied"</summary>
        public async Task<IActionResult> UploadUserAvatarImage([FromForm] UserAvatarImage form)
        {
            try
            {
                if (form.image == null)
                {
                    throw new ArgumentException("No image was given!");
                }
                var result = await _userService.UpdateUserAvatarAsync(form.image, form.userId);
                return result == null
                    ? StatusCode(StatusCodes.Status400BadRequest, new
                    {
                        Message = "Error updating, check log and form"
                    })
                    : Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPost("driverLicense/{userId}")]
        [SwaggerOperation(Summary = "Don't FromForm the IFormFile as it's already implied")]
        ///<summary>"Don't FromForm the IFormFile as it's already implied"</summary>
        public async Task<IActionResult> UploadDriverLicenseImage([FromForm] UploadDriverLicenses form)
        {
            try
            {
                if (form.images == null || form.images.Count <= 0)
                {
                    throw new ArgumentException("No image was given!");
                }
                var result = await _licenseService.UpdateDriverLicenseAsync(form.images, form.userId);
                return result == null
                    ? StatusCode(StatusCodes.Status400BadRequest, new
                    {
                        Message = "Error updating, check log and form"
                    })
                    : Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = ex.Message
                });
            }
        }
    }
}
