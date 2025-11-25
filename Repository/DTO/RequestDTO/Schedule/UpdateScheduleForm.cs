using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.Schedule
{
    public class UpdateScheduleForm
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? Location { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ScheduleType { get; set; }
        public int Priority { get; set; }

        public string? Note { get; set; }

        public bool IsBlocking { get; set; }

        public Guid CarId { get; set; }
    }
}
