using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data.Entities
{
    public class PaymentTransaction
    {
        public Guid Id { get; set; }
        public string GatewayName { get; set; }
        public string TransactionId { get; set; }
        public double Amount { get; set; }
        public DateTime PaidDate { get; set; }
        public string ResponseData { get; set; }

        public string Status { get; set; }

        public Guid PaymentHistoryId { get; set; }

        [ForeignKey("PaymentHistoryId")]
        public virtual PaymentHistory Payment { get; set; }
    }
}
