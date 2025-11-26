using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.Schedule
{
    public class CICOForm
    {
        public Guid UserId { get; set; }
        public Guid CarId { get; set; }
        public Guid BookingId { get; set; }
        public List<IFormFile> images { get; set; }
    }
}
