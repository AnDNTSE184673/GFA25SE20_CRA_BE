using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.Payment
{
    public class PayOSPaymentRequest
    {
        public int orderCode { get; set; }
        public int amount { get; set; }
        public string description { get; set; }
        public string buyerName { get; set; }

    }
}
