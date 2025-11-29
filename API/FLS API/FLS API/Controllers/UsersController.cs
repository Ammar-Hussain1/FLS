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
        public async Task<IActionResult> SignUp([FromBody] UsersDTO userDto)
        {
            var existing = await _supabaseService.GetUserByEmailAsync(userDto.Email);
            if (existing != null)
                return BadRequest("User already exists.");

            var user = await _supabaseService.SignUpUserAsync(userDto.FullName, userDto.Email, userDto.Password);
            return Ok(user);
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] UsersDTO userDto)
        {
            await _supabaseService.SignInUserAsync(userDto.Email, userDto.Password);
            return Ok("Magic link sent to email.");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _supabaseService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UsersDTO dto)
        {
            var user = await _supabaseService.UpdateUserAsync(id, dto);
            if (user == null) return NotFound();
            return Ok(user);
        }

    }
}
