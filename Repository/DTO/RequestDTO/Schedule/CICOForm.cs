using Microsoft.AspNetCore.Http;
using Repository.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.Schedule
{
    public class CICOForm
    {
        public Guid BookingId { get; set; }
        public Guid ResponsibleStaffId { get; set; }
        public string Description { get; set; }
        [MaxFileCount(10)]
        public List<IFormFile> images { get; set; }
    }
}
