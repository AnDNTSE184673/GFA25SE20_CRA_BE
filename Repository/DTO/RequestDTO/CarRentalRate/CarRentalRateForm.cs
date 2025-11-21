using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.CarRentalRate
{
    public class CarRentalRateForm
    {
        public double? DailyRate { get; set; }
        public double? HourlyRate { get; set; }
        public double? WeeklyDiscount { get; set; }
        public double? MonthlyDiscount { get; set; }
        public double? OvertimeRate { get; set; }
        public string Status { get; set; }

        public Guid CarId { get; set; }
    }
}
