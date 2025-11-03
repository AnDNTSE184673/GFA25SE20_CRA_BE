using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data.Entities
{
    public class ParkingLot
    {
        public Guid Id { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longtitude { get; set; }
        public int Capacity { get; set; }
        public string ContactNum { get; set; }
        public string? Notes { get; set; }

        public string Status { get; set; }

        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User Manager { get; set; }

        public virtual ICollection<Car> Cars { get; set; } = new List<Car>();
    }
}
