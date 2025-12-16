using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.ResponseDTO
{
    public class PersitNotifyReturn
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public bool IsViewed { get; set; }
        public DateTime CreateDate { get; set; }
        public Guid UserId { get; set; }
    }
}
