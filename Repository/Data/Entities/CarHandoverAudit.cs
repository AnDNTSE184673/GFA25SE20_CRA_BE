using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data.Entities
{
    public class CarHandoverAudit
    {
        [Key]
        public Guid Id { get; set; }
        
        public string Type { get; set; }  //CheckIn or CheckOut

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string Description { get; set; } = string.Empty;
        public string VerificationMethod { get; set; } = "OTP"; //or MFA, etc

        public string Location { get; set; }

        public Guid? ScheduleId { get; set; }
        public Guid ResponsibleStaffId { get; set; }

        [ForeignKey("ScheduleId")]
        public virtual Schedules Schedule { get; set; }
        [ForeignKey("ResponsibleStaffId")]
        public virtual User Staff { get; set; }
    }
}
