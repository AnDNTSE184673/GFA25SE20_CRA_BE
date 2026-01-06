using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data.Entities
{
    public class CarWallet
    {
        public Guid Id { get; set; }
        public Guid CarId { get; set; }
        public decimal Balance { get; set; }
        [ForeignKey("CarId")]
        public virtual Car Car { get; set; }
    }
}
