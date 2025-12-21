using Microsoft.AspNetCore.Mvc;
using Repository.Extension.TextBeeDotDev;


namespace CRA_Self_drive_Rental.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SMSController : ControllerBase
    {
        private readonly TextBeeSMSAPI _sms;

        public SMSController(TextBeeSMSAPI sms)
        {
            _sms = sms;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendSMSMessage(string message, string recipientPhone)
        {
            _sms.SendSMSMessage(message, recipientPhone);
            return Ok(new
            {
                Message = "Check phone for message!"
            });
        }
    }
}
