using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.CarRentalRate
{
    public class CarRentalRateForm
    {
        //infer one to another
        public double? DailyRate { get; set; }
        public double? HourlyRate { get; set; }
        [Range(0, 100, ErrorMessage = "Weekly Discount must be between 0% and 100%.")]
        public double? WeeklyDiscount { get; set; }
        [Range(0, 100, ErrorMessage = "Monthly Discount must be between 0% and 100%.")]
        public double? MonthlyDiscount { get; set; }
        [Range(0, 100, ErrorMessage = "Overtime rate must be between 0% and 100%.")]
        public double OvertimeRate { get; set; }

        public string Status { get; set; }

        public Guid CarId { get; set; }

        public (bool valid, bool isHourly) IsValid()
        {
            bool dataFilled = (DailyRate.HasValue && DailyRate.Value > 0) ||
                              (HourlyRate.HasValue && HourlyRate.Value > 0);
            bool isHourly = HourlyRate.HasValue && HourlyRate.Value > 0;

            return (dataFilled, isHourly);
        }
    }
}
