using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.PersitNotification
{
    public class NotifiCreateRequest
    {
        public string Content { get; set; }
        public Guid UserId { get; set; }
    }
}
