using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.CarToll
{
    public class CarWalletRequest
    {
        public Guid CarId { get; set; }
        public decimal Amount { get; set; }
    }
}
