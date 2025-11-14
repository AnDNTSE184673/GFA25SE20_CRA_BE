using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO
{
    public class InvoiceCreateRequest
    {
        [Required]
        public Guid CustomerId { get; set; }
        public Guid VendorId { get; set; }
        [Required]
        public Guid CarId { get; set; }
        [Required]
        public double CarRate { get; set; }
        [Required]
        public double Fees { get; set; }
        [Required]
        public int RentTime { get; set; }
        [Required]
        public string RentType { get; set; }
        [Required]
        public DateTime InvoiceDue { get; set; }
    }
}
