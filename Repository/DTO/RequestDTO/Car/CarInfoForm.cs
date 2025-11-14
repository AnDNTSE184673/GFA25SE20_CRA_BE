using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.Car
{
    public class CarInfoForm
    {
        public string Model { get; set; }
        public string Manufacturer { get; set; }
        public string Color { get; set; }
        public string LicensePlate { get; set; }
        public int Seats { get; set; }
        public string CarType { get; set; }
        public string? Features { get; set; }
        public string? Notes { get; set; }

        public Guid? UserId { get; set; }
        public string? Username { get; set; }
        public Guid? PrefLotId { get; set; }
        public string? PrefLotName { get; set; }
    }
}
