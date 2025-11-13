using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.ParkingLot
{
    public class PostParkingLotForm
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public double Latitude { get; set; }
        public double Longtitude { get; set; }
        public int Capacity { get; set; }
        public string ContactNum { get; set; }
        public string? Notes { get; set; }

        public Guid? ManagerId { get; set; }
        public string? ManagerName { get; set; }
    }
}
