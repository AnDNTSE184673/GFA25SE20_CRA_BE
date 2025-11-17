using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data.Entities
{
    public class CarRentalRate
    {
        [Key]
        public int Id { get; set; }
        public double DailyRate { get; set; }
        public double HourlyRate { get; set; }
        public double WeeklyDiscount { get; set; }
        public double MonthlyDiscount { get; set; }
        public double OvertimeRate { get; set; }
        public string Status { get; set; }

        public Guid CarId { get; set; }

        [ForeignKey("CarId")]
        public virtual Car Car { get; set; }

    }
}
