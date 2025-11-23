using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data.Entities
{
    public class Feedback
    {
        [Key]
        public Guid Id { get; set; }
        public double Rating { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreateDate { get; set; }

        public Guid CarId { get; set; }
        public Guid BookingId { get; set; }

        [ForeignKey("CarId")]
        public virtual Car Car { get; set; }
        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }

        public virtual ICollection<FeedbackImage> FeedbackImages { get; set; }
    }
}
