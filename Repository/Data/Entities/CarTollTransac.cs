using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data.Entities
{
    public class CarTollTransac
    {
        public Guid Id { get; set; }
        public long OrderCode { get; set; }
        public DateTime TransacDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
    }
}
