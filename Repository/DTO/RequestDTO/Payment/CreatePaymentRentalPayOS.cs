using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.Payment
{
    public class CreatePaymentRentalPayOS
    {
        public Guid BookingId { get; set; }
        public Guid PaymentId { get; set; }
    }
}
