using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data.Entities
{
    public class Contract
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
        public DateTime ValidUntil { get; set; }

        public string? TermsDetail { get; set; } //maybe OCR scan the doc and get the text here

        public string Status { get; set; }

        public Guid PartyAId { get; set; }
        public Guid PartyBId { get; set; }
        public Guid BookingId { get; set; }

        [ForeignKey("PartyAId")]
        public virtual User PartyA { get; set; }
        [ForeignKey("PartyBId")]
        public virtual User PartyB { get; set; }
        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }
    }
}
