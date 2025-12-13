using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO
{
    public class BookingExtensionRequest
    {
        [Required]
        public Guid BookingId { get; set; }
        [Required]
        public Guid CarId { get; set; }
        [Required]
        [Range(1, 30, ErrorMessage = "Extension time must be between 1 and 30 days.")]
        public int TimeExtInDays { get; set; }
        public string? Note { get; set; }
    }
}
