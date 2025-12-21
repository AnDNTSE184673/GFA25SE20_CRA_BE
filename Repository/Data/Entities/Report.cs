using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data.Entities
{
    public class Report
    {
        [Key]
        public Guid Id { get; set; }
        public string ReportNo { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public DateTime CreateDate { get; set; }
        public string Status { get; set; }

        public Guid? ReportedCarId { get; set; }
        public Guid? ReportedUserId { get; set; }
        public Guid ReporterId { get; set; }
        [ForeignKey("ReportedCarId")]
        public Car Car { get; set; }
        [ForeignKey("ReportedUserId")]
        public User Reported { get; set; }
        [ForeignKey("ReporterId")]
        public User Reporter { get; set; }
    }
}
