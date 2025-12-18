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

        public Guid CarId { get; set; }
        public Guid UserId { get; set; }
        [ForeignKey("CarId")]
        public Car Car { get; set; }
        [ForeignKey("UserId")]
        public User Reporter { get; set; }
    }
}
