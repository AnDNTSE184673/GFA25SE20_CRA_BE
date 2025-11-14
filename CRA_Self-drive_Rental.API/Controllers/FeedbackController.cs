using Microsoft.AspNetCore.Mvc;

namespace CRA_Self_drive_Rental.API.Controllers
{
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackServ;

        public FeedbackController(IFeedbackService feedbackServ)
        {
            _feedbackServ = feedbackServ;
        }

        public async Task<IActionResult> CreateFeedback(FeedbackForm form)
        {

        }
    }
}
