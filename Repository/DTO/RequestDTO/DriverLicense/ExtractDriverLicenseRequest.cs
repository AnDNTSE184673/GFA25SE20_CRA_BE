using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.DriverLicense
{
    public class ExtractDriverLicenseRequest
    {
        [Required]
        public IFormFile Image { get; set; }
    }
}
