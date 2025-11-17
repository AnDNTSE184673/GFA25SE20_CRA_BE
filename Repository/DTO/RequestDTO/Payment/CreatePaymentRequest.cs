using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.Payment
{
    public class CreatePaymentRequest
    {
        public Guid TransactionId { get; set; } // PaymentHistory.Id
        public double Amount { get; set; }
        public Guid InvoiceId { get; set; }
        public Guid UserId { get; set; }
        public string PayerName { get; set; }
    }
}
