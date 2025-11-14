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

        public (bool valid, bool isId) IsValid()
        {
            bool pair1Filled = !string.IsNullOrEmpty(FilePath) && !string.IsNullOrEmpty(Bucket);
            bool pair2Filled = CarId.HasValue && UserId.HasValue;

            bool valid = pair1Filled || pair2Filled;
            bool isId = pair2Filled; //true if ID pair is filled, false if string pair

            return (valid, isId);
        }
    }
    
}
