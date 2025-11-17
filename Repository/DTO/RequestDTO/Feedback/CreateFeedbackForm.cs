using Microsoft.AspNetCore.Http;
using Repository.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.Feedback
{
    public class CreateFeedbackForm
    {
        public double Rating { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        [MaxFileCount(5)]
        public List<IFormFile> Medias { get; set; }

        public Guid CarId { get; set; }
        public Guid BookingId { get; set; }
    }
}
