using Microsoft.AspNetCore.Http;
using Repository.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.Report
{
    public class CarReportForm
    {
        public string Title { get; set; }
        public string Content { get; set; }

        [Required]
        public Guid ReporterId { get; set; }

        public Guid ReportedCarId { get; set; }
    }
}
