using Repository.DTO.ResponseDTO.Car;
using Repository.DTO.ResponseDTO.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.ResponseDTO.Booking
{
    public class BookingView
    {
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string Status { get; set; }
        public string InvoiceNo { get; set; }
        public UserView User { get; set; }
        public CarView Car { get; set; }
    }
}
