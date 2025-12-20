using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.ResponseDTO.GPS
{
    /// <summary>
    /// GPS telemetry payload.
    /// Matches: { carId, latitude, longitude, speed, timestamp }
    /// </summary>
    public class GpsPayload
    {
        [Required]
        public Guid CarId { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        // nullable to match "pos.coords.speed || 0" semantics from JS
        public double? Speed { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }
    }
}
