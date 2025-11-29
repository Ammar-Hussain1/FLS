using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FLS_API.BL;
using FLS_API.DL.DTOs;

namespace FLS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly SupabaseService _supabaseService;

        public UsersController(SupabaseService supabaseService)
        {
            _supabaseService = supabaseService;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpDTO signUpDto)
        {
            if (signUpDto == null)
                return BadRequest("Request body is required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate that email is not null or empty
            if (string.IsNullOrWhiteSpace(signUpDto.Email))
                return BadRequest("Email is required.");

            if (string.IsNullOrWhiteSpace(signUpDto.Password))
                return BadRequest("Password is required.");

            if (string.IsNullOrWhiteSpace(signUpDto.FullName))
                return BadRequest("Full name is required.");

            // Ensure we're using the actual email from the request, not any default value
            var email = signUpDto.Email.Trim().ToLower();
            var fullName = signUpDto.FullName.Trim();
            var password = signUpDto.Password;

            try
            {
                var user = await _supabaseService.SignUpUserAsync(fullName, email, password);
                return Ok(user);
            }
            catch (Exception ex)
            {   
                // Return a user-friendly error message
                return BadRequest($"Error during signup: {ex.Message}");
            }
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignInDTO signInDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _supabaseService.SignInUserAsync(signInDto.Email, signInDto.Password);
            if (user == null)
                return Unauthorized("Invalid email or password.");

            return Ok(user);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _supabaseService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _supabaseService.UpdateUserAsync(id, dto);
            if (user == null) return NotFound();
            return Ok(user);
        }

    }
}
