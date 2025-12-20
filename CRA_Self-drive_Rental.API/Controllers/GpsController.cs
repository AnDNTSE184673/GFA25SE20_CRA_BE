using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Repository.DTO.RequestDTO;
using Repository.DTO.ResponseDTO.GPS;
using Service.Infranstructure;
using Service.Services;

namespace CRA_Self_drive_Rental.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GpsController : ControllerBase
    {
        private readonly ILogger<GpsController> _logger;
        private readonly IGpsStore _store;
        private readonly IGPSService _gpsService;

        public GpsController(ILogger<GpsController> logger, IGpsStore store, IGPSService gPSService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _gpsService = gPSService ?? throw new ArgumentNullException(nameof(gPSService));
        }

        /// <summary>
        /// Receive a single GPS telemetry ping for a car.
        /// </summary>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(202)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        public IActionResult ReceiveTelemetry([FromBody] GpsPayload payload)
        {
            if (payload == null) return BadRequest();

            // store last-known telemetry
            _store.Upsert(payload);

            _logger.LogInformation("Received GPS telemetry for car {CarId} at {Timestamp}: {@Payload}",
                payload.CarId, payload.Timestamp, new { payload.Latitude, payload.Longitude, Speed = payload.Speed ?? 0.0 });

            return Accepted();
        }

        /// <summary>
        /// Receive a single GPS telemetry ping for a car from Device.
        /// </summary>
        [HttpPost("/FromDevice")]
        public async Task<IActionResult> ReceiveTelementry([FromBody] GPSReceive receive)
        {
            if (!ModelState.IsValid) return BadRequest();
            var result = await _gpsService.AddGPS(receive);
            if (result == null) return BadRequest();
            return Ok(result);
        }


        /// <summary>
        /// Get All GPS Telementries
        /// </summary>
        [HttpGet("/All")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _gpsService.GetAllAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get All GPS Telementries for a Car
        /// </summary>
        [HttpGet("/ByCar/{carId}")]
        public async Task<IActionResult> GetForCar(Guid carId)
        {
            if (carId  == Guid.Empty) return BadRequest();
            var result = await _gpsService.GetByCarIdAsync(carId);
            if (result == null) return BadRequest();
            return Ok(result);
        }

        /// <summary>
        /// Get All GPS Telementries for a User
        /// </summary>
        [HttpGet("/ByUser/{userId}")]
        public async Task<IActionResult> GetForUser(Guid userId)
        {
            if (userId == Guid.Empty) return BadRequest();
            var result = await _gpsService.GetByUserIdAsync(userId);
            if (result == null) return BadRequest();
            return Ok(result);
        }

        /// <summary>
        /// Get All GPS Telementries for a Device
        /// </summary>
        [HttpGet("/ByDevice/{deviceId}")]
        public async Task<IActionResult> GetForDevice(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId)) return BadRequest();
            var result = await _gpsService.GetByDeviceIdAsync(deviceId);
            if (result == null) return BadRequest();
            return Ok(result);
        }

        /// <summary>
        /// Delete all GPS telemetries from a car.
        /// </summary>
        [HttpDelete("/Car/{carId}")]
        public async Task<IActionResult> DeleteAllGFromCar(Guid carId)
        {
            if (carId == Guid.Empty) return BadRequest();
            var result = await _gpsService.DeleteGPSOfCar(carId);
            if (result == 0) return BadRequest();
            return Ok(result);
        }


        /// <summary>
        /// Get last known telemetry for a car.
        /// </summary>
        [HttpGet("{carId:guid}")]
        [ProducesResponseType(typeof(GpsPayload), 200)]
        [ProducesResponseType(404)]
        public IActionResult GetLastForCar(Guid carId)
        {
            if (_store.TryGet(carId, out var payload))
                return Ok(payload);

            return NotFound(new { Message = "No telemetry for car", CarId = carId });
        }

        /// <summary>
        /// Get last known telemetry for all cars (in-memory).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(GpsPayload[]), 200)]
        public IActionResult GetAllLast()
        {
            var all = _store.GetAll();
            return Ok(all);
        }
    }
}
