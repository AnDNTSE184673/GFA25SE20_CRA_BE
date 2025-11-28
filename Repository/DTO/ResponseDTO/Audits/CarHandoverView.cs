using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.ResponseDTO.Audits
{
    public class CarHandoverView
    {
        public Guid Id { get; set; }

        public string Type { get; set; }

        public DateTime Timestamp { get; set; }

        public string Description { get; set; }
        public string VerificationMethod { get; set; }

        public Guid ScheduleId { get; set; }
        public Guid ResponsibleStaffId { get; set; }
    }
}
