using Microsoft.AspNetCore.Http;
using Repository.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.CarRegister
{
    public class CarRegForm
    {
        //public IFormFile image { get; set; }
        public Guid CarId { get; set; }
        public Guid UserId { get; set; }

        [MaxFileCount(10)]
        public List<IFormFile> images { get; set; }
    }
}
