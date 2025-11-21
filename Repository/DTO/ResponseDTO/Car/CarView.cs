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
        public Guid Id { get; set; }
        public string LicensePlate { get; set; }
        public string Model { get; set; }
        public string Manufacturer { get; set; }
        public int Seats { get; set; }
        public int YearofManufacture { get; set; }
        public string Transmission { get; set; }
        public string FuelType { get; set; }
        public double FuelConsumption { get; set; }
        public string? Description { get; set; }
        public double Rating { get; set; }

        public string Status { get; set; }

        public UserView Owner { get; set; }
        public ParkingLotView PreferredLot { get; set; }

        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}
