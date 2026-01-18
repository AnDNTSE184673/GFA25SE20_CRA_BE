using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.ResponseDTO.Car
{
    public class CarTravelView
    {
        public Guid CarId { get; set; }
        public Guid BookingId { get; set; }
        public DateTime TravelDate { get; set; }
        public int TollBoothId { get; set; }
        public string TollBoothName { get; set; }
        public decimal ChargeAmount { get; set; }
    }
}
