using Microsoft.AspNetCore.Http;
using Repository.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.Inquiry
{
    public class AnswerInquiryForm
    {
        public string? Title { get; set; }
        public string Content { get; set; }

        public bool isOpen { get; set; }

        [MaxFileCount(10)]
        public List<IFormFile> Medias { get; set; } = new List<IFormFile>();

        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public Guid? ParentInquiryId { get; set; }
    }
}
