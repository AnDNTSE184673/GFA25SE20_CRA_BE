using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Repository.DTO.ResponseDTO.GPS;
using Service.Infranstructure;

namespace CRA_Self_drive_Rental.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GpsController : ControllerBase
    {
        private readonly ILogger<GpsController> _logger;
        private readonly IGpsStore _store;

        public GpsController(ILogger<GpsController> logger, IGpsStore store)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _store = store ?? throw new ArgumentNullException(nameof(store));
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
