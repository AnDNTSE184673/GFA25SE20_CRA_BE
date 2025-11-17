using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Extension
{
    public class MaxFileCountAttribute : ValidationAttribute
    {
        private readonly int _max;

        public MaxFileCountAttribute(int max)
        {
            _max = max;
            ErrorMessage = $"You can upload a maximum of {_max} files.";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var files = value as IList<IFormFile>;
            if (files != null && files.Count > _max)
                return new ValidationResult(ErrorMessage);

            return ValidationResult.Success;
        }
    }

}
