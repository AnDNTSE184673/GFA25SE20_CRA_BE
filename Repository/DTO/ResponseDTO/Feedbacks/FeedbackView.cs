using Repository.DTO.ResponseDTO.Car;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.ResponseDTO.Feedbacks
{
    public class FeedbackView
    {
        public double Rating { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreateDate { get; set; }

        //get images

        public CarView car { get; set; }
        public Guid BookingId { get; set; }
    }
}
