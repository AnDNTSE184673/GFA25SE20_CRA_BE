using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.ResponseDTO.Payment
{
    public class PaymentHistoryView
    {
        public Guid Id { get; set; }
        public long OrderCode { get; set; }
        public string Item { get; set; }
        public double PaidAmount { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public Guid InvoiceId { get; set; }
        public Guid UserId { get; set; }
    }
}
