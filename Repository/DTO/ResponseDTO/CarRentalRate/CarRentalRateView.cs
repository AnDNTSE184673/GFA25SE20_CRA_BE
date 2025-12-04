using Repository.DTO.ResponseDTO.Car;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.ResponseDTO.CarRentalRate
{
    public class CarRentalRateView
    {
        public decimal? DailyRate { get; set; }
        public decimal? HourlyRate { get; set; }

        public double? WeeklyDiscount { get; set; }
        public double? MonthlyDiscount { get; set; }

        public int? MaxDistancePerDay { get; set; }
        public decimal? OvertravelRatePerKm { get; set; }

        public string Status { get; set; }

        public Guid CarId { get; set; }
    }
}
