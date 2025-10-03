using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO
{
    public class RefreshTokensRequest
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }

    public class RefreshTokensResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
