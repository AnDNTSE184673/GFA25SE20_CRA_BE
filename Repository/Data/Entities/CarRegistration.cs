using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data.Entities
{
    public class CarRegistration
    {
        public int Id { get; set; }
        public string FilePath { get; set; }
        public string Bucket { get; set; }
        public DateTime UrlExpiration { get; set; }
        public DateTime CreateDate { get; set; }

        public string Status { get; set; }

        public Guid CarId { get; set; }
        public Guid UserId { get; set; }

        [ForeignKey("CarId")]
        public virtual Car Car { get; set; }
        [ForeignKey("UserId")]
        public virtual User Owner { get; set; }
    }
}
