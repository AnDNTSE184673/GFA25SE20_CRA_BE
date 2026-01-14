using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.ResponseDTO.CarToll
{
    public class CarTollView
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public Guid CarId { get; set; }
        public decimal Total { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public virtual ICollection<CarTollTransacView> CarTollTransacs { get; set; } = new List<CarTollTransacView>();
    }
}
