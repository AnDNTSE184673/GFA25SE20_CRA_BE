using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.Schedule
{
    public class CICOImageSearch
    {
        public Guid BookingId { get; set; }
        public bool isCheckIn { get; set; }
    }
}
