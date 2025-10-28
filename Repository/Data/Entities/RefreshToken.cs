using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Data.Entities
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        public string? JWTRefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
        public DateTime? RefreshTokenCreated { get; set; }
        public DateTime? RevokedAt { get; set; }

        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
