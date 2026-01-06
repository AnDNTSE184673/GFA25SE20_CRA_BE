using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Services;
using System.Threading.Tasks;

namespace CRA_Self_drive_Rental.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarWalletController : ControllerBase
    {
        private readonly ICarWalletService _walletService;
        public CarWalletController(ICarWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _walletService.GetAllCarWallets();
            return Ok(result);
        }

        [HttpGet("/{Id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            if (id == Guid.Empty) return BadRequest();
            var result = await _walletService.GetbyId(id);
            if (result == null) return BadRequest();
            return Ok(result);
        }

        [HttpGet("/Car/{carId}")]
        public async Task<IActionResult> GetByCar(Guid carId)
        {
            if (carId == Guid.Empty) return BadRequest();
            var result = await _walletService.GetCarWalletByCarId(carId);
            if (result == null) return BadRequest();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewWallet(Guid carId)
        {
            if (carId == Guid.Empty) return BadRequest();
            var result = await _walletService.CreateCarWallet(carId);
            if (result == null) return BadRequest();
            return Ok(result);
        }

        [HttpPut("/Car/{carId}")]
        public async Task<IActionResult> UpdateWallet([FromForm]Guid carId, decimal amount)
        {
            if (carId == Guid.Empty) return BadRequest();
            if (amount <= 0) return BadRequest();
            var result = await _walletService.UpdateCarWalletBalance(carId, amount);
            if (result == null) return BadRequest();
            return Ok(result);
        }

        [HttpPut("/Car/{carId}/AddWithAmount")]
        public async Task<IActionResult> AddToWallet([FromForm] Guid carId, decimal amount)
        {
            if (carId == Guid.Empty) return BadRequest();
            if (amount <= 0) return BadRequest();
            var result = await _walletService.AddToCarWallet(carId, amount);
            if (result == null) return BadRequest();
            return Ok(result);
        }

        [HttpPut("/Car/{carId}/SubtractWithAmount")]
        public async Task<IActionResult> SubtractFromWallet([FromForm] Guid carId, decimal amount)
        {
            if (carId == Guid.Empty) return BadRequest();
            if (amount <= 0) return BadRequest();
            var result = await _walletService.SubtractFromCarWallet(carId, amount);
            if (result == null) return BadRequest();
            return Ok(result);
        }

        [HttpDelete("/Car/{carId}")]
        public async Task<IActionResult> DeleteWallet(Guid carId)
        {
            if (carId == Guid.Empty) return BadRequest();
            var result = await _walletService.DeleteCarWallet(carId);
            return Ok(result);
        }
    }
}
