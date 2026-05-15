using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.Constant;
using Repository.DTO.RequestDTO.Contract;
using Repository.Extension.SupabaseFileUploader;

namespace CRA_Self_drive_Rental.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractController : ControllerBase
    {
        private readonly IContractService _contractServ;

        public ContractController(IContractService contractServ)
        {
            _contractServ = contractServ;
        }

        public async Task<IActionResult> UploadContractDocument([FromForm]ContractUploadDTO form)
        {
            try
            {
                if (form.Documents == null || form.Documents.Count <= 0)
                {
                    throw new ArgumentException("No document was given!");
                }

                var allowedExtensions = new HashSet<string>
                {
                    ".pdf", ".doc",".docx"
                };

                var maxFileSizeInMBs = 50;

                var imageValidationOptions = FileValidationPolicyFactory
                    .CreateFromExtensions(
                        allowedExtensions,
                        maxFileSizeInMBs
                );

                foreach (var i in form.Documents)
                {
                    var frontResult = FileValidationHelper.Validate(i, imageValidationOptions);
                    if (!frontResult.IsValid)
                        return BadRequest(frontResult.Error);
                }

                var result = await _contractServ.UploadDocuments(form);
                return result.status.Contains(ConstantEnum.RepoStatus.FAILURE)
                    ? StatusCode(500, new
                    {
                        Message = "Error, check log and form"
                    })
                    : Ok(result.regDoc);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = ex.Message
                });
            }
        }
    }
}
