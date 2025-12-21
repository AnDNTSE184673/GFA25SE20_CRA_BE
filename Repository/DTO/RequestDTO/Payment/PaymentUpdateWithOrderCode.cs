using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.Payment
{
    public class PaymentUpdateWithOrderCode
    {
        [Required]
        public long OrderCode { get; set; }
        public string Status { get; set; }
        public string Method { get; set; }
    }
}
