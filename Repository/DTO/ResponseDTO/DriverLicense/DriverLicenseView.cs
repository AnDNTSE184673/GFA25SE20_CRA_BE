using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.ResponseDTO.DriverLicense
{
    public class DriverLicenseView
    {
        public Guid? UserId { get; set; }

        public int Side { get; set; }

        public string? LicenseNumber { get; set; }
        public string? LicenseName { get; set; }
        public DateOnly? LicenseDoB { get; set; }
        public string? LicenseClass { get; set; }
        public DateOnly? LicenseIssue { get; set; }
        public DateOnly? LicenseExpiry { get; set; }

        public List<string> Urls { get; set; }

        public DateTime CreateDate { get; set; }

        public string Status { get; set; }
    }
}
