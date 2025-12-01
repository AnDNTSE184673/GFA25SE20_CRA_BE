using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data.Entities
{
    public class CarRentalRatePolicy
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        // Late return
        public int LateReturnGracePeriodMinutes { get; set; } = 15;
        public double? LateReturnFeePerHour { get; set; }

        public double HolidayDiscount { get; set; }

        public int CarRateId { get; set; }

        [ForeignKey("CarRateId")]
        public virtual CarRentalRate Rate { get; set; }
    }
}
