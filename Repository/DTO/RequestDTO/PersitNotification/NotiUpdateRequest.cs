using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.PersitNotification
{
    public class NotiUpdateRequest
    {
        public Guid Id { get; set; }
        public bool IsViewed { get; set; }
        public string? Content { get; set; }

    }
}
