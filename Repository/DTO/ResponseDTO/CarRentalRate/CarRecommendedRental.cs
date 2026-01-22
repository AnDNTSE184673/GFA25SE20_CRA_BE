using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.ResponseDTO.CarRentalRate
{
    public class CarRecommendedRental
    {
        public decimal RecommendedPrice { get; set; }
        public decimal RecommendedMinPrice { get; set; }
        public decimal RecommendedMaxPrice { get; set; }
    }
}
