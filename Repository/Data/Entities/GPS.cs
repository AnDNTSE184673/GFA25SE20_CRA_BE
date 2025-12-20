using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data.Entities
{
    public class GPS
    {
        public Guid Id { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public int? Speed { get; set; }
        public Guid CarId { get; set; }
        public Guid UserId { get; set; }
        public string DeviceId { get; set; }
        public DateTime Timestamp { get; set; }

        [ForeignKey("CarId")]
        public virtual Car Car { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
