using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Repository.Constant.ConstantEnum;

namespace Repository.DTO.RequestDTO
{
    public class BookingCreateRequest
    {
        public Guid CustomerId { get; set; }
        public Guid CarId { get; set; }
        public string PickupPlace { get; set; }
        public DateTime PickupTime { get; set; }
        public string DropoffPlace { get; set; }
        public DateTime DropoffTime { get; set; }
        [Range(15, double.MaxValue)]
        public double bookingFee { get; set; } = 15;
        public decimal carRentPrice { get; set; }
        public int rentime { get; set; }
        public string rentType { get; set; } = "Days"; // hour, day, week, month
    }
}
