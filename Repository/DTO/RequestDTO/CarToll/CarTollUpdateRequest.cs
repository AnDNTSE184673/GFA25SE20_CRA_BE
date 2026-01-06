using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.CarToll
{
    public class CarTollUpdateRequest
    {
        public Guid BookingId { get; set; }
        public string BookingNum { get; set; }
        public Guid CarId { get; set; }
        public decimal Total { get; set; }
    }
}
