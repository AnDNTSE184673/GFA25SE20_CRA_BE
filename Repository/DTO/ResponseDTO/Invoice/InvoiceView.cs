using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.ResponseDTO.Invoice
{
    public class InvoiceView
    {
        public Guid Id { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public double SubTotal { get; set; }
        public double GrandTotal { get; set; }
        public string? Note { get; set; }
        public DateTime CreateDate { get; set; }

        public string Status { get; set; }
        public virtual ICollection<InvoiceItemView> InvoiceItems { get; set; } = new List<InvoiceItemView>();
    }
}
