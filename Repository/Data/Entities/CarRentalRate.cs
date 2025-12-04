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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public decimal? DailyRate { get; set; }
        public decimal? HourlyRate { get; set; }

        public double? WeeklyDiscount { get; set; }
        public double? MonthlyDiscount { get; set; }

        public int? MaxDistancePerDay { get; set; }
        public decimal? OvertravelRatePerKm { get; set; }

        public string Status { get; set; }

        public Guid CarId { get; set; }
        /*
        public int CarPolicyId { get; set; }
        [ForeignKey("CarPolicyId")]
        public virtual CarRentalRatePolicy Policy { get; set; }
        */
        [ForeignKey("CarId")]
        public virtual Car Car { get; set; }
    }
}
