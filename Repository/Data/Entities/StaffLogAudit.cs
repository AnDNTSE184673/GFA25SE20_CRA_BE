using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data.Entities
{
    public class StaffLogAudit
    {
        [Key]
        public Guid Id { get; set; }

        public string Action { get; set; }

        public string? IpAddress { get; set; }
        public string UserAgent { get; set; } //var userAgent = Request.Headers["User-Agent"].ToString();

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public Guid? RelatedHandoverId { get; set; }
        public Guid StaffId { get; set; }

        [ForeignKey("StaffId")]
        public virtual User Staff { get; set; }
        [ForeignKey("RelatedHandoverId")]
        public virtual CarHandoverAudit Handover { get; set; }
    }

    //A Json HEader is like this
    //GET /api/checkin HTTP/1.1
    //Host: yourapi.com
    //User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) ...
    //Accept: application/json
}
