using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.ResponseDTO.Schedule
{
    public class CICOImageView
    {
        public Guid? BookingId { get; set; }

        public List<string> Urls { get; set; }
        public DateTime CreateDate { get; set; }

        public string Status { get; set; }
    }
}
