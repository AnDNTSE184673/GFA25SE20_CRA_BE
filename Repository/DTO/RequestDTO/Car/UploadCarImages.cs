using Microsoft.AspNetCore.Http;
using Repository.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.Car
{
    public class UploadCarImages
    {
        [MaxFileCount(10)]
        public List<IFormFile> images { get; set; }
        public Guid carId { get; set; }
    }
}
