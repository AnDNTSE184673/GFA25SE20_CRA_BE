using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.Payment
{
    public class UpdatePaymentRequest
    {
        [Required]
        public Guid PaymentId { get; set; }
        public string Status { get; set; }
    }
}
