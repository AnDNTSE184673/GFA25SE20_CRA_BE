using System;
using System.Collections.Generic;
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
        public Status status { get; set; }
        public double bookingFee { get; set; }
        public double carRentPrice { get; set; }
        public int rentime { get; set; }
        public string rentType { get; set; } = "hour"; // hour, day, week, month
        public DateTime rentDateEnd { get; set; }
    }
}
