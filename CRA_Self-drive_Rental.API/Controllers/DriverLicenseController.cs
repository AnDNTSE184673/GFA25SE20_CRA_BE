using Microsoft.AspNetCore.Mvc;
using Repository.Constant;
using Repository.DTO.RequestDTO.DriverLicense;
using Repository.DTO.ResponseDTO.DriverLicense;
using Repository.Extension.SupabaseFileUploader;
using Service.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace CRA_Self_drive_Rental.API.Controllers
{
    [Route("api/User/driverLicense")]
    [ApiController]
    public class DriverLicenseController : ControllerBase
    {
        private readonly IDriverLicenseService _licenseService;

        public DriverLicenseController(IDriverLicenseService licenseService)
        {
            _licenseService = licenseService;
        }

        [HttpPost("upload")]
        [SwaggerOperation(Summary = "Don't FromForm the IFormFile as it's already implied")]
        public async Task<IActionResult> UploadDriverLicenseImage([FromForm] UploadDriverLicenses form)
        {
            try
            {
                if (form.frontDriverLicenseimg == null)
                {
                    throw new ArgumentException("Front side of license needed!");
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

                var frontResult = FileValidationHelper.Validate(form.frontDriverLicenseimg, imageValidationOptions);
                if (!frontResult.IsValid)
                    return BadRequest(frontResult.Error);

                /*var backResult = FileValidationHelper.Validate(form.backDriverLicenseimg, imageValidationOptions);
                if (!backResult.IsValid)
                    return BadRequest(backResult.Error);*/

                var result = await _licenseService.UpdateDriverLicenseAsync(form.userId, form.frontDriverLicenseimg);
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

        [HttpGet]
        public async Task<IActionResult> GetDriverLicense([FromQuery] LicenseSearchForm form)
        {
            try
            {
                (string[] signedUrl, List<DriverLicenseView> view) result = (Array.Empty<string>(), new List<DriverLicenseView>());
                if (!form.IsValid())
                {
                    return BadRequest(new
                    {
                        Message = ModelState
                    });
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
                        //Urls = result.signedUrl,
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

        [HttpPatch("approve")]
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

        [HttpGet("all")]
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
    }
}