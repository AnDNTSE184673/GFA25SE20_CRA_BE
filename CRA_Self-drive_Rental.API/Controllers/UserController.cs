using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.Constant;
using Repository.DTO.RequestDTO;
using Repository.DTO.RequestDTO.CarRegister;
using Repository.DTO.RequestDTO.DriverLicense;
using Repository.DTO.RequestDTO.User;
using Repository.DTO.ResponseDTO.CarRegister;
using Repository.DTO.ResponseDTO.DriverLicense;
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

        [HttpGet("driverLicense")]
        ///<summary>Leave form blank to get all, fill to get specific</summary>
        public async Task<IActionResult> GetDriverLicense([FromQuery] LicenseSearchForm form)
        {
            try
            {
                (string[] signedUrl, List<DriverLicenseView> view) result = (Array.Empty<string>(), new List<DriverLicenseView>());
                if (!form.IsValid())
                {
                    result = await _licenseService.GetAllDocumentsAsync();
                }
                else
                {
                    result = await _licenseService.GetDriverLicenseByUser(form);
                }

                return result.view == null
                    ? StatusCode(404, new
                    {
                        Message = "No data found, check log"
                    })
                    : Ok(new
                    {
                        Urls = result.signedUrl,
                        View = result.view
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPatch("driverLicense/approve")]
        public async Task<IActionResult> ApproveDocument(LicenseSearchForm form, bool isApproved)
        {
            try
            {
                var validation = form.IsValid();

                if (!validation)
                    throw new InvalidOperationException("Fill 1 of 2 complete pairs of data");

                var result = await _licenseService.ApproveLicenseAsync(form, isApproved);

                return result.status.Equals(ConstantEnum.RepoStatus.FAILURE)
                    ? StatusCode(500, new
                    {
                        Message = "Data edit error, check log and form"
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

        [HttpGet("driverLicense/all")]
        public async Task<IActionResult> GetAllDocuments()
        {
            try
            {
                var result = await _licenseService.GetAllDocumentsAsync();
                return !result.view.Any()
                    ? StatusCode(StatusCodes.Status404NotFound, new
                    {
                        Message = "Data fetch error, check log and form"
                    })
                    : Ok(new
                    {
                        Urls = result.signedUrl,
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
    }
}
