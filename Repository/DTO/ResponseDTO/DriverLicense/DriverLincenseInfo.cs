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

        public int CheckValidation()
        {
            var score =
                IdProbability * 0.25 +
                DateOfExpiryProbability * 0.20 +
                DateOfBirthProbability * 0.15 +
                NameProbability * 0.20 +
                DateOfIssueProbability * 0.15 +
                ClassProbability * 0.05;
            int result = 0;
            if (score < 75) result = -1;
            else if (score >= 75 && score <= 90) result = 0;
            else result = 1;

            return result;
        }
    }

}
