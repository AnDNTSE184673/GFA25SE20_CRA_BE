using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.DriverLicense
{
    public class LicenseSearchForm
    {
        public Guid? UserId { get; set; }

        public string? Email { get; set; }

        public bool IsValid()
        {
            bool isValid = UserId.HasValue || !Email.IsNullOrEmpty();

            return isValid;
        }
    }
}
