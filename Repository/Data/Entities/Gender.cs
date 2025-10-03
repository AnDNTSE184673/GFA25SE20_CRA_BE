using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data.Entities
{
    public class Gender
    {
        [Key]
        public int Id { get; set; }
        public string GenderTitle { get; set; }
    }
}
