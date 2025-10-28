using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data.Entities
{
    public class Invoice
    {
        [Key]
        public Guid Id { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public double SubTotal { get; set; }
        public double GrandTotal { get; set; }
        public string? Note { get; set; }
        public DateTime CreateDate { get; set; }

        public string Status { get; set; }

        public Guid CustomerId { get; set; }
        public Guid VendorId { get; set; }

        [ForeignKey("CustomerId")]
        public virtual User Customer { get; set; }
        [ForeignKey("VendorId")]
        public virtual User Vendor { get; set; }

        public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
    }
}
