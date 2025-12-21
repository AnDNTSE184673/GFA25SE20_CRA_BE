using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.Payment
{
    public class CreateFromBookingOtherFee
    {
        public Guid BookingId { get; set; }
        public string Description { get; set; }

        private decimal amount;
        public decimal Amount
        {
            get { return amount; }
            set { amount = value / 10; }
        }
    }
}
