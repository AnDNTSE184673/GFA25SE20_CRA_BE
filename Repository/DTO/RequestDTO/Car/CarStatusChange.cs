using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.Car
{
    public class CarStatusChange
    {
        public Guid? carId { get; set; }
        public string? LicensePlate { get; set; }

        public bool isActive { get; set; }

        public bool IsValid()
        {
            var valid = carId.HasValue || !LicensePlate.IsNullOrEmpty();

            return valid;
        }
    }
}
