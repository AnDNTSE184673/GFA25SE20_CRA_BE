using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data.Entities
{
    public class Booking
    {
        public Guid Id { get; set; }

        public string PickupPlace { get; set; }
        public DateTime PickupTime { get; set; }
       
        public string DropoffPlace { get; set; }
        public DateTime DropoffTime { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }

        public string Status { get; set; }

        public Guid UserId { get; set; }
        public Guid CarId { get; set; }
        public Guid InvoiceId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
        [ForeignKey("InvoiceId")]
        public Invoice Invoice { get; set; }
        [ForeignKey("CarId")]
        public Car Car { get; set; }
    }
}
