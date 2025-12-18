using Microsoft.AspNetCore.Http;
using Repository.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.Report
{
    public class ReportForm
    {
        public string Title { get; set; }
        public string Content { get; set; }

        public Guid CarId { get; set; }
        public Guid UserId { get; set; }
    }
}
