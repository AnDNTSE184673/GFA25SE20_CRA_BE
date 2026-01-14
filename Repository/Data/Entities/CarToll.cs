using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data.Entities
{
    public class CarToll
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public string BookingNum { get; set; }
        public Guid CarId { get; set; }
        public decimal Total { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }
        [ForeignKey("CarId")]
        public virtual Car Car { get; set; }
        public virtual ICollection<CarTollTransac> CarTollTransacs { get; set; } = new List<CarTollTransac>();
    }
}
