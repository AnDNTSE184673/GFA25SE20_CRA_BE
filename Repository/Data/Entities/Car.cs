using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data.Entities
{
    public class Car
    {
        [Key]
        public Guid Id { get; set; }
        public string LicensePlate { get; set; }
        public string Model { get; set; }
        public string Manufacturer { get; set; }
        public int Seats { get; set; }
        public int YearofManufacture { get; set; }
        public string Transmission { get; set; }
        public string FuelType { get; set; }
        public double FuelConsumption { get; set; }
        public string? Description { get; set; }

        public double Rating { get; set; }

        public string Status { get; set; }

        public Guid UserId { get; set; }
        public Guid PrefLotId { get; set; }
        public Guid LotId { get; set; }

        [ForeignKey("UserId")]
        public virtual User Owner { get; set; }
        [ForeignKey("PrefLotId")]
        public virtual ParkingLot PreferredLot { get; set; }

        public virtual ICollection<CarImage> Images { get; set; }
    }
}
