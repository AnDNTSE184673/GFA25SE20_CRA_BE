using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.ResponseDTO.Report
{
    public class ReportView
    {
        public Guid Id { get; set; }
        public string ReportNo { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public DateTime CreateDate { get; set; }
        public string Status { get; set; }

        public Guid CarId { get; set; }
        public Guid UserId { get; set; }
    }
}
