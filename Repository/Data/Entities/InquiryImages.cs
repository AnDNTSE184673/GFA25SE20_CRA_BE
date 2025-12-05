using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data.Entities
{
    public class InquiryImages
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Bucket { get; set; }

        public long? FileSize { get; set; }
        public string? MimeType { get; set; }

        public DateTime CreateDate { get; set; }

        public string Status { get; set; }

        public Guid InquiryId { get; set; }

        [ForeignKey("InquiryId")]
        public virtual Inquiry Inquiry { get; set; }
    }
}
