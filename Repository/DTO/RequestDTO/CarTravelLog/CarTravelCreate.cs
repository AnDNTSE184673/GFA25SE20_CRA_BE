using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.CarTravelLog
{
    public class CarTravelCreate
    {
        [Required]
        public Guid CarId { get; set; }
        [Required]
        public Guid BookingId { get; set; }
        [Range(1, 4)]
        [Required]
        public int TollBoothId { get; set; }
    }
}
