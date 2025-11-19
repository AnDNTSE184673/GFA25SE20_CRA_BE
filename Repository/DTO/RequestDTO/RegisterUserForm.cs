using Repository.Constant;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO
{
    public class RegisterUserForm
    {
        [Required]
        //[RegularExpression(@"^[a-zA-Z0-9\sÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚĂĐĨŨƠàáâãèéêìíòóôõùúăđĩũơƯĂẮẰẲẴẶắằẳẵặƯứừửữự]+$",
        //    ErrorMessage = "UserName must not contain special characters.")]
        public string Username { get; set; }
        public string? TitleName { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "Password must be at least 8 characters long", MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[!@#$%^&*]).+$",
            ErrorMessage = "Password must contains a special character, a number, an uppercase and lowercase letter")]
        public string Password { get; set; }
        [RegularExpression(@"^0\d{9,12}$",
        //[RegularExpression(@"^\+?[\d\s\-\(\)\.]+$",
            ErrorMessage = "Not a phone number format")]
        public string? PhoneNumber { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string? Fullname { get; set; }
        public int? RoleId { get; set; } = (int)ConstantEnum.RoleID.CUSTOMER;
        [Required]
        public int? GenderId { get; set; } = (int)ConstantEnum.GenderID.OTHER;
        public string? GoogleId { get; set; }
    }
}
