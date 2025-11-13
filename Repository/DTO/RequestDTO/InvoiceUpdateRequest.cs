using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Repository.Constant.ConstantEnum;

namespace Repository.DTO.RequestDTO
{
    public class InvoiceUpdateRequest
    {
        [Required]
        public Guid Id { get; set; }
        public Status status { get; set; }
        public DateTime DueDate { get; set; }
    }
}
