using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.ResponseDTO.Contract
{
    public class ContractViewDTO
    {
        public Guid BookingId { get; set; }
        public Guid PartyA { get; set; }
        public Guid PartyB { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime ValidUntil { get; set; }

        public List<string> DocUrls { get; set; } = new List<string>();
    }
}
