using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO
{
    public class BookingUpdateRequest
    {
        [Required]
        public Guid BookingId { get; set; }
        [Required]
        public string? Status { get; set; }
    }
}
