using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.Map
{
    public class DistanceRequest
    {
        [Required]
        public string SourceAddress { get; set; }
        [Required]
        public string DestinationAddress { get; set; }
    }
}
