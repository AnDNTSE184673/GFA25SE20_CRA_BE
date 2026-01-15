using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data.Entities
{
    public class CarTravelLog
    {
        public int Id { get; set; }
        public Guid CarId { get; set; }
        public Guid BookingId { get; set; }
        public DateTime TravelDate { get; set; }
        public int TollBoothId { get; set; }
        public decimal ChargeAmount { get; set; }
        [ForeignKey("CarId")]
        public virtual Car Car { get; set; }
        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }
        [ForeignKey("TollBoothId")]
        public virtual TollBooth TollBooth { get; set; }
    }
}
