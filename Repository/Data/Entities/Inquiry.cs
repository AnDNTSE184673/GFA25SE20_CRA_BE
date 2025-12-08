using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data.Entities
{
    public class Inquiry
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string Content { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }

        public string Status { get; set; }
        public string Type { get; set; }

        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public Guid? ParentInquiryId { get; set; }

        [ForeignKey("SenderId")]
        public virtual User Sender { get; set; }
        [ForeignKey("ReceiverId")]
        public virtual User Receiver { get; set; }
        [ForeignKey("ParentInquiryId")]
        public virtual Inquiry ParentInquiry { get; set; }

        public virtual ICollection<InquiryImages> InquiryImages { get; set; }
        public virtual ICollection<Inquiry> Replies { get; set; }
    }
}
