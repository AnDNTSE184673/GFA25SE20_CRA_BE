using Supabase.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.CarRegister
{
    public class GetCarRegForm
    {
        public string? FilePath { get; set; }
        public string? Bucket { get; set; }

        public Guid? CarId { get; set; }
        public Guid? UserId { get; set; }

        public string? Email { get; set; }
        public string? LicensePlate { get; set; }

        public (bool valid, bool isId, bool isPath, bool isInfo) IsValid()
        {
            bool pair2Filled = !string.IsNullOrEmpty(FilePath) && !string.IsNullOrEmpty(Bucket);
            bool pair1Filled = CarId.HasValue && UserId.HasValue;
            bool pair3Filled = !string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(LicensePlate);

            bool valid = pair1Filled || pair2Filled || pair3Filled;

            return (valid, pair1Filled, pair2Filled, pair3Filled);
        }
    }
    
}
