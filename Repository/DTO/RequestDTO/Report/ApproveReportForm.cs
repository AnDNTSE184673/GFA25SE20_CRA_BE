using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.Report
{
    public class ApproveReportForm
    {
        public Guid? ReportId { get; set; }
        public string? ReportNo { get; set; }

        public bool isApproved { get; set; }

        public bool IsValid()
        {
            bool valid = ReportId.HasValue || !ReportNo.IsNullOrEmpty();

            return valid;
        }
    }
}
