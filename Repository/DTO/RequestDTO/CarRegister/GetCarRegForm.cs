using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.CarRegister
{
    public class GetCarRegForm
    {
        public string FilePath { get; set; }
        public string Bucket { get; set; }
        public Guid CarId { get; set; }
        public Guid UserId { get; set; }
    }
}
