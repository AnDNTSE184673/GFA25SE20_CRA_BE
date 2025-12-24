using Microsoft.AspNetCore.Http;
using Repository.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.Schedule
{
    public class CheckInOutImages
    {
        [MaxFileCount(15)]
        public List<IFormFile> images { get; set; } = new List<IFormFile>();
        public Guid bookingId { get; set; }
        public bool isCheckIn { get; set; } //checkIn is true checkOut is false
    }
}
