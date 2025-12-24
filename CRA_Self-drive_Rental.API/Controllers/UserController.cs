using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using Repository.Constant;
using Repository.DTO.RequestDTO;
using Repository.DTO.RequestDTO.CarRegister;
using Repository.DTO.RequestDTO.DriverLicense;
using Repository.DTO.RequestDTO.User;
using Repository.DTO.ResponseDTO.CarRegister;
using Repository.DTO.ResponseDTO.DriverLicense;
using Repository.Extension.SupabaseFileUploader;
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

        public UserController(IUserService userService)
        {
            _userService = userService;
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

        [HttpPatch("reset-user-reputation")]
        public async Task<IActionResult> ResetUserReputation(Guid userId)
        {
            try 
            {
                var response = await _userService.ResetUserReputation(userId);
                return response == null
                    ? StatusCode(StatusCodes.Status400BadRequest, new
                    {
                        Message = "Error updating, check log and form"
                    })
                    : Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = ex.Message
                });
            }  
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetUserPassword([FromBody] UpdatePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Invalid update password data",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }
            var response = await _userService.UpdateUserPasswordAsync(request);
            return Ok(new
            {
                Message = response
            });
        }

        [HttpPost("reset-password/verify")]
        public async Task<IActionResult> OTPVerificationPassword(string email, string OTPCode)
        {
            var response = await _userService.AuthorizeUpdateUserPasswordAsync(email, OTPCode);
            if (response == null) return BadRequest(new
            {
                Message = "Incorrect OTP code!"
            });
            return Ok(new
            {
                Message = response
            });
        }

        [HttpPost("change-phoneNo")]
        public async Task<IActionResult> ChangeUserPhoneNumber([FromBody] UpdatePhoneNumberRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Invalid update phone number data",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }
            var response = await _userService.UpdateUserPhoneNumber(request);
            return Ok(new
            {
                Message = response
            });
        }

        [HttpPost("change-phoneNo/verify")]
        public async Task<IActionResult> OTPVerificationPhone(string phone, string OTPCode)
        {
            var response = await _userService.AuthorizeUpdateUserPhoneNumberAsync(phone, OTPCode);
            if (response == null) return BadRequest(new
            {
                Message = "Incorrect OTP code!"
            });
            return Ok(new
            {
                Message = response
            });
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

                var allowedExtensions = new HashSet<string>
                {
                    ".jpg", ".jpeg", ".png"
                };

                var maxFileSizeInMBs = 50;

                var imageValidationOptions = FileValidationPolicyFactory
                    .CreateFromExtensions(
                        allowedExtensions,
                        maxFileSizeInMBs
                );

                var validateResult = FileValidationHelper.Validate(form.image, imageValidationOptions);
                if (!validateResult.IsValid)
                    return BadRequest(validateResult.Error);

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
    }
}
