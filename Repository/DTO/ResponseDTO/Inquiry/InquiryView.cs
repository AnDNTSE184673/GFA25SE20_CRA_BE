using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.ResponseDTO.Inquiry
{
    public class InquiryView
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }

        public string Status { get; set; }
        public string Type { get; set; }

        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public Guid? ParentInquiryId { get; set; }

        public List<string> ImageUrls { get; set; }
        public List<InquiryView> PreviousInquiry { get; set; }
    }
}
