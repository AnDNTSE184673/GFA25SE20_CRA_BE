using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data.Entities
{
    public class PaymentHistory
    {
        public Guid Id { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long OrderCode { get; set; }
        public string Item { get; set; }
        public double PaidAmount { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string PaymentProofUrl { get; set; }
        public string PaymentMethod { get; set; }        
        public string Note { get; set; }
        public string? Signature { get; set; }
        public string Status { get; set; }

        public Guid InvoiceId { get; set; }
        public Guid UserId { get; set; }

        [ForeignKey("InvoiceId")]
        public virtual Invoice Invoice { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
