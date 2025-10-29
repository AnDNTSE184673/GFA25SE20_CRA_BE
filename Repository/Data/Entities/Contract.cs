using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data.Entities
{
    public class Contract
    {
        public int Id { get; set; }
        public string DocUrl { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ValidUntil { get; set; }

        public string Status { get; set; }

        public Guid PartyAId { get; set; }
        public Guid PartyBId { get; set; }
        public Guid InvoiceId { get; set; }

        [ForeignKey("PartyAId")]
        public virtual User PartyA { get; set; }
        [ForeignKey("PartyBId")]
        public virtual User PartyB { get; set; }
        [ForeignKey("InvoiceId")]
        public virtual Invoice Invoice { get; set; }
    }
}
