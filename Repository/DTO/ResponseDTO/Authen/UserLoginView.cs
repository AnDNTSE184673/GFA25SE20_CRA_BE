using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.DTO.ResponseDTO.User
{
    public class UserLoginView
    {
        public string Username { get; set; }
        public string Email { get; set; }

        public string IsGoogle { get; set; }

        public string RoleName { get; set; }

        public string JwtToken { get; set; }
        public string RefreshToken { get; set; }
    }
}