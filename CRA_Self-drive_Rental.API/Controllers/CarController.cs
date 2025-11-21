using Microsoft.AspNetCore.Mvc;
using Repository.Base;
using Repository.Constant;
using Repository.Data.Entities;
using Repository.DTO.RequestDTO.Car;
using Repository.DTO.RequestDTO.CarRegister;
using Repository.DTO.ResponseDTO.CarRegister;
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

        [HttpPatch("regDoc/approve")]
        public async Task<IActionResult> ApproveDocument(DocumentSearchForm form, bool isApproved)
        {
            try
            {
                var validation = form.IsValid();

                if (!validation.valid)
                    throw new InvalidOperationException("Fill 1 of 2 complete pairs of data");

                var result = await _carRegServ.ApproveDocumentsAsync(form, isApproved);

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
            
        [HttpGet("regDoc/all")]
        public async Task<IActionResult> GetAllDocuments()
        {
            try
            {
                var result = await _carRegServ.GetAllDocumentsAsync();
                return result.Count <= 0
                    ? StatusCode(500, new
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

        [HttpPost("registerCar/carInfo")]
        public async Task<IActionResult> AddCarInfo([FromForm] CarInfoForm form)
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
        public async Task<IActionResult> UploadRegistrationImage([FromForm] CarRegForm form)
        {
            try
            {
                if (form.images == null || form.images.Count <= 0)
                {
                    throw new ArgumentException("No image was given!");
                }
                var result = await _carRegServ.SubmitRegisterDocument(form);
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

        [HttpGet("regDoc")]
        ///<summary>Also send a flag indicating whether to search using "path" or "id" or "info"</summary>
        public async Task<IActionResult> GetCarRegistration([FromQuery] GetCarRegForm form, string flag)
        {
            try
            {
                (string[] signedUrl, List<CarRegView> view) result = (Array.Empty<string>(), new List<CarRegView>());
                var validation = form.IsValid();

                if (!validation.valid)
                    throw new InvalidOperationException("Fill 1 of 3 complete pairs of data");

                if (!flag.Contains(ConstantEnum.InternalFlag.IdSearch)
                    || !flag.Contains(ConstantEnum.InternalFlag.PathSearch)
                    || !flag.Contains(ConstantEnum.InternalFlag.InfoSearch))
                    throw new InvalidOperationException("Choose a search method by setting flag 'flag','id', or 'info'.");

                if (flag.Contains(ConstantEnum.InternalFlag.IdSearch) && validation.isId)
                {
                    result = await _carRegServ.GetCarRegDocById(form);
                }

                else if (flag.Contains(ConstantEnum.InternalFlag.InfoSearch) && validation.isInfo)
                {
                    result = await _carRegServ.GetCarRegDocByInfo(form);
                }

                else if (flag.Contains(ConstantEnum.InternalFlag.PathSearch) && validation.isPath)
                {
                    result = await _carRegServ.GetCarRegDocByPath(form);
                }

                else
                {
                    throw new InvalidOperationException("No valid combination of flags and filled pairs found");
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

        [HttpGet("AllCars")]
        public async Task<IActionResult> GetAllCars()
        {
            try
            {
                var cars = await _carServ.GetAllCarsAsync();
                return Ok(cars);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = ex.Message
                });
            }
        }

        [HttpGet("{carId}")]
        public async Task<IActionResult> GetCarById(Guid carId)
        {
            try
            {
                var car = await _carServ.GetCarByIdAsync(carId);
                if (car == null)
                {
                    return NotFound(new
                    {
                        Message = "Car not found"
                    });
                }
                return Ok(car);
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
