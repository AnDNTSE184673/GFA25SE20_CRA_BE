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
        public string Model { get; set; }
        public string Manufacturer { get; set; }
        public string Color { get; set; }
        public string LicensePlate { get; set; }
        public int Seats { get; set; }
        public string CarType { get; set; }
        public string? Features { get; set; }
        public double Rating { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User Owner { get; set; }
    }
}
