using Microsoft.AspNetCore.Http;
using Repository.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.Car
{
    public class CarInfoForm
    {
        [Required]
        public string LicensePlate { get; set; }
        [Required]
        [StringLength(100)]
        public string Model { get; set; }
        [Required]
        [StringLength(100)]
        public string Manufacturer { get; set; }
        [Range(2, 20, ErrorMessage = "Seats must be between 2 and 20.")]
        public int Seats { get; set; }
        [Range(1900, 2100, ErrorMessage = "Year must be between 1900 and the current year.")]
        // If you want EXACT current-year enforcement, you must validate that manually in your service layer.
        public int YearofManufacture { get; set; }
        [Required]
        [StringLength(50)]
        public string Transmission { get; set; }  // could later be restricted to enum
        [Required]
        [StringLength(50)]
        public string FuelType { get; set; } // could also be an enum
        [Range(0.1, 100, ErrorMessage = "Fuel consumption must be between 0.1 and 100 L/100km.")]
        public double FuelConsumption { get; set; }
        [StringLength(1000, ErrorMessage = "Description must be 1000 characters or less.")]
        public string? Description { get; set; }


        [MaxFileCount(5)]
        public List<IFormFile> Medias { get; set; }

        public Guid? UserId { get; set; }
        public string? Username { get; set; }
        public Guid? PrefLotId { get; set; }
        public string? PrefLotName { get; set; }
    }
}
