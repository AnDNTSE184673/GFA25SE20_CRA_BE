using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.Car
{
    public class SearchCarForm
    {
        public string? Model { get; set; }
        public string? Manufacturer { get; set; }
        public int? Seats { get; set; }
        public int? YearofManufacture { get; set; }
        public string? Transmission { get; set; }
        public string? FuelType { get; set; }

        public string? Description { get; set; }

        public double? Rating { get; set; }
    }
}
