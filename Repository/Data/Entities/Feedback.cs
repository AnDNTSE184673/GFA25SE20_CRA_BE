using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public string Content { get; set; }

        public Guid CarId { get; set; }
        public Guid BookingId { get; set; }
    }
}
