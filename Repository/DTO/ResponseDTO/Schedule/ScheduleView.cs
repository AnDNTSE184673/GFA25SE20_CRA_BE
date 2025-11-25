using Repository.DTO.ResponseDTO.Booking;
using Repository.DTO.ResponseDTO.Car;
using Repository.DTO.ResponseDTO.User;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.ResponseDTO.Schedule
{
    public class ScheduleView
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ScheduleType { get; set; }
        public int Priority { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }

        public string? Note { get; set; }

        public bool IsBlocking { get; set; }

        public string Status { get; set; }

        public CarView Car { get; set; }
        public UserView? User { get; set; }
        public BookingView? Booking { get; set; }
    }
}
