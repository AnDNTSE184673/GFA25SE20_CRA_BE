using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.ResponseDTO
{
    public class OTPView
    {
        public int Id { get; set; }

        public Guid UserId { get; set; }
        public string OtpHash { get; set; }

        public DateTime ExpirationTime { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsUsed { get; set; } = false;
        public int AttemptCount { get; set; } = 0;
    }
}
