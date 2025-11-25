using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.Schedule
{
    public class MaintenanceSchedule
    {
        public string Title { get; set; }

        public string? Location { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string? Note { get; set; }

        public Guid CarId { get; set; }
    }
}
