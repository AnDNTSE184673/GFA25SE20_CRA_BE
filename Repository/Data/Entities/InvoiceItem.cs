﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data.Entities
{
    public class InvoiceItem
    {
        public Guid Id { get; set; }
        public string Item { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double Total { get; set; }
        public string? Note { get; set; }

        public Guid InvoiceId { get; set; }

        [ForeignKey("InvoiceId")]
        public virtual Invoice Invoice { get; set; }
    }
}
