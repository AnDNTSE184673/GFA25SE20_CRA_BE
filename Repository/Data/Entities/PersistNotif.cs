using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data.Entities
{
    public class PersistNotif
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public bool IsViewed { get; set; }
        public DateTime CreateDate { get; set; }

        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
