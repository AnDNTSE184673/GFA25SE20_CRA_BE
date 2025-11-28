using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.ResponseDTO.Audits
{
    public class StaffLogView
    {
        public Guid Id { get; set; }

        public string Action { get; set; }

        public string? IpAddress { get; set; }
        public string UserAgent { get; set; }

        public DateTime Timestamp { get; set; }

        public Guid? RelatedHandoverId { get; set; }
        public Guid StaffId { get; set; }
    }
}
