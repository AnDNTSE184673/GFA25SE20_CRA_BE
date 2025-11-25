using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.User
{
    public class UserAvatarImage
    {
        public IFormFile image { get; set; }
        public Guid userId { get; set; }
    }
}
