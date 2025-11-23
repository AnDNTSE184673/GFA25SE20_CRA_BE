using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.ResponseDTO.CarRegister
{
    public class CarRegView
    {
        public Guid? CarId { get; set; }
        public Guid? UserId { get; set; }

        public List<string> Urls { get; set; }
        public DateTime CreateDate { get; set; }

        public string Status { get; set; }    
    }
}
