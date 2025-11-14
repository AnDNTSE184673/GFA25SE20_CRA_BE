using Repository.Data.Entities;
using Repository.DTO.ResponseDTO.ParkingLot;
using Repository.DTO.ResponseDTO.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.ResponseDTO.Car
{
    public class CarView
    {
        public string Model { get; set; }
        public string Manufacturer { get; set; }
        public string Color { get; set; }
        public string LicensePlate { get; set; }
        public int Seats { get; set; }
        public string CarType { get; set; }
        public string? Features { get; set; }
        public string? Notes { get; set; }
        public double Rating { get; set; }

        public string Status { get; set; }

        public UserView Owner { get; set; }
        public ParkingLotView PreferredLot { get; set; }
    }
}
