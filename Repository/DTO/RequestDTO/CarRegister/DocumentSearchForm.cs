using Supabase.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.CarRegister
{
    public class DocumentSearchForm
    {
        public Guid? UserId { get; set; }
        public Guid? CarId { get; set; }

        public string? Email { get; set; }
        public string? LicensePlate { get; set; } 

        public (bool valid, bool isId) IsValid()
        {
            bool pair1Filled = !string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(LicensePlate);
            bool pair2Filled = CarId.HasValue && UserId.HasValue;

            bool valid = pair1Filled || pair2Filled;
            bool isId = pair2Filled; //true if ID pair is filled, false if string pair

            return (valid, isId);
        }
    }
}
