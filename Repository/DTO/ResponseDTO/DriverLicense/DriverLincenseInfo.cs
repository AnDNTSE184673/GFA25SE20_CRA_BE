using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.ResponseDTO.DriverLicense
{
    public class DriverLincenseInfo
    {
        public string LicenseId { get; set; }
        public int IdProbability { get; set; }
        public string NameOnLicense { get; set; }
        public int NameProbability { get; set; }
        public string Class { get; set; }
        public int ClassProbability { get; set; }
        public string DateOfBirth { get; set; }
        public int DateOfBirthProbability { get; set; }
        public string DateOfIssue { get; set; }
        public int DateOfIssueProbability { get; set; }
        public string DateOfExpiry { get; set; }
        public int DateOfExpiryProbability { get; set; }
    }
}
