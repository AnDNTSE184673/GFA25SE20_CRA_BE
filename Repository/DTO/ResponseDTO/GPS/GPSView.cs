using Repository.DTO.ResponseDTO.Car;
using Repository.DTO.ResponseDTO.User;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.ResponseDTO.GPS
{
    public class GPSView
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int? Speed { get; set; }
        public Guid UserId { get; set; }
        public string DeviceId { get; set; }
        public DateTime Timestamp { get; set; }
        public virtual UserView User { get; set; }
    }
}
