using Microsoft.AspNetCore.Http;
using Repository.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.DriverLicense
{
    public class UploadDriverLicenses
    {
        public IFormFile frontDriverLicenseimg { get; set; }
        //public IFormFile backDriverLicenseimg { get; set; }
        public Guid userId { get; set; }
    }
}
