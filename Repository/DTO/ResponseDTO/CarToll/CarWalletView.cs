using Repository.DTO.ResponseDTO.Car;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.ResponseDTO.CarToll
{
    public class CarWalletView
    {
        public Guid Id { get; set; }
        public Guid CarId { get; set; }
        public decimal Balance { get; set; }
    }
}
