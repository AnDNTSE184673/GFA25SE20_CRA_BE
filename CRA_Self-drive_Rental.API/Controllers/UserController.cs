using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.DTO.RequestDTO;
using Service.Services;

namespace CRA_Self_drive_Rental.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }


        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var response = await _userService.GetAllUsers();
            return Ok(response);
        }

        [HttpGet("GetUserById")]
        public async Task<IActionResult> GetUserById([FromQuery] Guid userId)
        {
            var response = await _userService.GetUserById(userId);
            if (response == null)
            {
                return NotFound("User not found.");
            }
            return Ok(response);
        }

        [HttpPatch("UpdateUserInfo")]
        public async Task<IActionResult> UpdateUserInfo([FromBody] UserUpdateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Invalid update data",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }
            var response = await _userService.UpdateUserInfo(request);
            return Ok(response);
        }
    }
}
