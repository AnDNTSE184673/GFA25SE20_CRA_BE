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
        [MaxFileCount(5)]
        public List<IFormFile> images { get; set; } = new List<IFormFile>();
        public Guid userId { get; set; }
    }
}
