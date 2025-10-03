using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data.Entities
{
    public class Invoice
    {
        [Key]
        public Guid Id { get; set; }
        public int InvoiceNo { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public double SubTotal { get; set; }
        public double GrandTotal { get; set; }
        public string Note { get; set; }
        public DateTime CreateDate { get; set; }
        
        public int StatusId { get; set; }
        public int CurrencyId { get; set; }

    }
}
