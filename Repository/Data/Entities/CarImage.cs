using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data.Entities
{
    public class CarImage
    {
        public int Id { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Bucket { get; set; }

        public long? FileSize { get; set; }
        public string? MimeType { get; set; }

        public DateTime CreateDate { get; set; }

        public string Status { get; set; }

        public Guid CarId { get; set; }

        [ForeignKey("CarId")]
        public virtual Car Car { get; set; }
    }
}
