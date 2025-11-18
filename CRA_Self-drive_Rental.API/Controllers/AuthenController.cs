using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Repository.CustomFunctions.TokenHandler;
using Repository.DTO;
using Repository.DTO.RequestDTO;
using Service.Services;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace CRA_Self_drive_Rental.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly JWTTokenProvider _jwt;
        public AuthenController(IUserService userService, JWTTokenProvider jWT)
        {
            _userService = userService;
            _jwt = jWT;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Email and password must be provided.");
            }
            var response = await _userService.AuthenticateAsync(request.Email.Trim(), request.Password);
            if (response == null)
            {
                return Unauthorized("Invalid email or password.");
            }
            return Ok(response);
        }

        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp([FromBody] RegisterRequest register)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Invalid registration data",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }
            var response = await _userService.RegisterCustomer(register);
            // Implementation for user sign-up goes here
            return Ok(response);
        }

        /// <param name="localURL">https://localhost:7184/Authen/login/google</param>
        [SwaggerOperation(Summary = "Copy the local url and open it on a new tab (testing)")]
        [HttpGet("login/google")]
        public async Task<IActionResult> GoogleLogin(string? localURL)
        {
            var redirUrl = Url.Action("GoogleResponse", "Authen", null);
            var request = new AuthenticationProperties { RedirectUri = redirUrl };
            return Challenge(request, "Google");
        }

        /// <summary>Don't run this</summary>
        [HttpGet("google-callback")] // This must match the authorized redir uri in google cloud console oauth client
        public async Task<IActionResult> GoogleResponse()
        {
            try
            {
                var result = await HttpContext.AuthenticateAsync("Google");
                if (!result.Succeeded)
                    return Unauthorized();
                if (result.Principal == null)
                    return Unauthorized();

                var email = result.Principal.FindFirst(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
                var name = result.Principal.FindFirst(c => c.Type == System.Security.Claims.ClaimTypes.Name)?.Value;
                var googleId = result.Principal.FindFirst(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (email.IsNullOrEmpty())
                    return BadRequest();
                //service LoginGoogle if no acc regster Google take it email nam googleiD
                var response = await _userService.GoogleLogin(email, name, googleId);
                //check response.Status
                if (response.login == null && response.register == null)
                    throw new Exception("Something went wrong, contact admin");
                else if (response.login != null)
                    return Ok(response.login);
                else if (response.register != null)
                    return Ok(response.register);
                else
                    throw new Exception("Something went wrong, contact admin");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = ex.Message
                });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokensRequest form)
        {
            var principal = _jwt.GetPrincipalFromExpiredToken(form.AccessToken);
            if (principal == null) return BadRequest("Invalid access token");

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userService.GetUserWithToken(Guid.Parse(userId));

            if (user == null || user.RefreshTokens.Last().JWTRefreshToken != form.RefreshToken || user.RefreshTokens.Last().RefreshTokenExpiry < DateTime.UtcNow)
                return Unauthorized();

            // Call the method here
            var tokens = _jwt.RefreshTokenAsync(user);

            // Update user's refresh token in DB
            _jwt.RefreshTokenAsync(user);

            return Ok(tokens);

        }
    }
}
