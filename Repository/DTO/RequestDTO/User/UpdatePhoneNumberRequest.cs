using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.User
{
    public class UpdatePhoneNumberRequest
    {
        [Required]
        public Guid userId { get; set; }
        [Required]
        [Phone]
        public string newPhoneNumber { get; set; }
    }
}
