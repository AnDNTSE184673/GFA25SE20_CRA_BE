using Microsoft.AspNetCore.Mvc;
using Repository.Base;
using Repository.Constant;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO.Car;
using Repository.DTO.RequestDTO.CarRegister;
using Repository.Repositories.Interfaces;
using Service.Services;
using Service.Services.Implementation;
using Swashbuckle.AspNetCore.Annotations;

namespace CRA_Self_drive_Rental.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarController : ControllerBase
    {
        private readonly ICarService _carServ;
        private readonly ICarRegService _carRegServ;

        public CarController(ICarService carServ, ICarRegService carRegServ)
        {
            _carServ = carServ;
            _carRegServ = carRegServ;
        }

        [HttpPost("registerCar/carInfo")]
        public async Task<IActionResult> AddCarInfo(CarInfoForm form)
        {
            try
            {
                var result = await _carServ.RegisterCarAsync(form);
                return result.status.Contains(ConstantEnum.RepoStatus.FAILURE)
                    ? StatusCode(500, new
                    {
                        Message = "Data creation error, check log and form"
                    })
                    : Ok(result.car);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = ex.Message
                });
            }
        }

        [HttpPost("registerCar/regDoc")]
        [SwaggerOperation(Summary = "Don't FromForm the IFormFile as it's already implied")]
        ///<summary>"Don't FromForm the IFormFile as it's already implied"</summary>
        public async Task<IActionResult> UploadImage(IFormFile image, [FromForm] CarRegForm form)
        {
            try
            {
                if (image == null)
                {
                    throw new ArgumentException("No image was given!");
                }
                var result = await _carRegServ.SubmitRegisterDocument(image, form);
                return result.status.Contains(ConstantEnum.RepoStatus.FAILURE)
                    ? StatusCode(500, new
                    {
                        Message = "Error, check log and form"
                    })
                    : Ok(result.regDoc);
            }
            catch (Exception ex)
            { 
                return StatusCode(StatusCodes.Status500InternalServerError, new 
                { 
                    message = ex.Message 
                });
            }
        }

        [HttpGet("carRegDoc")]
        public async Task<IActionResult> GetCarRegistration(GetCarRegForm form)
        {
            try
            {
                var result = await _carRegServ.GetCarRegDoc(form);
            }
            catch(Exception ex)
            {

            }
        }
    }
}
