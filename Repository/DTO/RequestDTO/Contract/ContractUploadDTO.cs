using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.Contract
{
    public class ContractUploadDTO
    {
        public string? TermDetails { get; set; }
        public List<IFormFile> Documents { get; set; } = new List<IFormFile>();

        public DateTime ValidDate { get; set; }

        public Guid BookingId { get; set; }
        public Guid PartyA { get; set; }
        public Guid PartyB { get; set; }
    }
}
