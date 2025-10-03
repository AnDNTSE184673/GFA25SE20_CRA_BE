using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data.Entities
{
    public class Role
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }

        public virtual ICollection<User>? Users { get; set; } = new List<User>();
    }
}
