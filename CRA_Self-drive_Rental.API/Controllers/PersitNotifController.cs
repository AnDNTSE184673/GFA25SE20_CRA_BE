using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.DTO.RequestDTO.PersitNotification;
using Service.Services;

namespace CRA_Self_drive_Rental.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersitNotifController : ControllerBase
    {
        private readonly IPersitNotifService _persitNotifService;
        public PersitNotifController(IPersitNotifService persitNotifService)
        {
            _persitNotifService = persitNotifService;
        }

        [HttpGet("/AllNotif")]
        public async Task<IActionResult> GetAllNotif()
        {
            var result = await _persitNotifService.GetAllNotif();
            return Ok(result);
        }

        [HttpGet("/UserNotif/{userId}")]
        public async Task<IActionResult> GetUserNotif(Guid userId)
        {
            var result = await _persitNotifService.GetNotifByUserId(userId);
            return Ok(result);
        }

        [HttpGet("/Notif/{id}")]
        public async Task<IActionResult> GetNotifById(Guid id)
        {
            var result = await _persitNotifService.GetNotifById(id);
            return Ok(result);
        }

        [HttpPost("/CreateNotif")]
        public async Task<IActionResult> CreateNotif([FromBody] NotifiCreateRequest input)
        {
            var result = await _persitNotifService.CreateNotif(input);
            return Ok(result);
        }

        [HttpPut("/UpdateNotif")]
        public async Task<IActionResult> UpdateNotif([FromBody] NotiUpdateRequest input)
        {
            var result = await _persitNotifService.UpdateNotif(input);
            return Ok(result);
        }

        [HttpPatch("/MarkAsRead/{id}")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var result = await _persitNotifService.MarkAsRead(id);
            return Ok(result);
        }

        [HttpDelete("/DeleteNotif/{id}")]
        public async Task<IActionResult> DeleteNotif(Guid id)
        {
            var result = await _persitNotifService.DeleteNotif(id);
            return Ok(result);
        }

        [HttpDelete("/DeleteNotifsByUser/{userId}")]
        public async Task<IActionResult> DeleteNotifsByUserId(Guid userId)
        {
            var result = await _persitNotifService.DeleteNotifsByUserId(userId);
            return Ok(result);
        }
    }
}
