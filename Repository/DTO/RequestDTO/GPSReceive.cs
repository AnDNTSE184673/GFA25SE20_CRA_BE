using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO
{
    public class GPSReceive
    {
        [Required]
        public double Latitude { get; set; }
        [Required]
        public double Longitude { get; set; }
        public int? Speed { get; set; }
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public string DeviceId { get; set; }
    }
}
