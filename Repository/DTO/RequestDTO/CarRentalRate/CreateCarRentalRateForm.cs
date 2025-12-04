using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.CarRentalRate
{
    public class CreateCarRentalRateForm
    {
        //infer one to another
        public decimal? DailyRate { get; set; }
        public decimal? HourlyRate { get; set; }

        [Range(0, 75, ErrorMessage = "Weekly Discount must be between 0% and 75%.")]
        public double? WeeklyDiscount { get; set; }

        [Range(0, 75, ErrorMessage = "Monthly Discount must be between 0% and 75%.")]
        public double? MonthlyDiscount { get; set; }

        [Range(300, 500, ErrorMessage = "Max distance must be between 300km and 500km.")]
        public int MaxDistancePerDay { get; set; }

        public decimal OvertravelRatePerKmInDongperKM { get; set; }

        public Guid CarId { get; set; }

        //could consider passing userid to check ownerz

        public (bool valid, bool isHourly) IsValid()
        {
            bool dataFilled = (DailyRate.HasValue && DailyRate.Value > 0) ||
                              (HourlyRate.HasValue && HourlyRate.Value > 0);
            bool isHourly = HourlyRate.HasValue && HourlyRate.Value > 0;

            return (dataFilled, isHourly);
        }
    }
}
