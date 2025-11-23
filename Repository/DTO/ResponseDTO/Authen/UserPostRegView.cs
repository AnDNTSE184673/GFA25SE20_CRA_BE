using Repository.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.DTO.ResponseDTO.User
{
    public class UserPostRegView
    { 
        public string Username { get; set; } 
        public string? Password { get; set; }   
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Fullname { get; set; }
        public string? Address { get; set; }

        public bool IsCarOwner { get; set; }

        public string Status { get; set; }

        public string RoleName { get; set; }
        public string IsGoogle { get; set; }
    }
}