using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO.RequestDTO.Feedback
{
    public class EditFeedbackForm
    { 
        public double Rating { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }
}
