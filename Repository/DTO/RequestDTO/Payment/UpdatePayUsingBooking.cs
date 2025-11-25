using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.Payment
{
    public class UpdatePayUsingBooking
    {
        [Required]
        public Guid BookingId { get; set; }
        [Required]
        public string status { get; set; }
    }
}
