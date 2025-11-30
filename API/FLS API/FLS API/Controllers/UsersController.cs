using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FLS_API.BL;
using FLS_API.DL.DTOs;
using FLS_API.DTOs;
using FLS_API.Utilities;

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
                return ApiResponse.ErrorResponse("Request body is required.", "INVALID_REQUEST").ToActionResult();

            if (!ModelState.IsValid)
                return ApiResponse.ErrorResponse("Invalid request data.", "VALIDATION_ERROR").ToActionResult();

            var email = signUpDto.Email?.Trim().ToLower() ?? string.Empty;
            var fullName = signUpDto.FullName?.Trim() ?? string.Empty;
            var password = signUpDto.Password ?? string.Empty;

            var result = await _supabaseService.SignUpUserAsync(fullName, email, password);
            return result.ToActionResult(StatusCodes.Status201Created);
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignInDTO signInDto)
        {
            if (!ModelState.IsValid)
                return ApiResponse.ErrorResponse("Invalid request data.", "VALIDATION_ERROR").ToActionResult();

            var result = await _supabaseService.SignInUserAsync(signInDto.Email, signInDto.Password);
            return result.ToActionResult();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _supabaseService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _supabaseService.GetUserByIdAsync(id);
            if (user == null)
            {
                return ApiResponse.ErrorResponse("User not found.", "USER_NOT_FOUND").ToActionResult();
            }
            return ApiResponse.SuccessResponse(user).ToActionResult();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDTO dto)
        {
            if (!ModelState.IsValid)
                return ApiResponse.ErrorResponse("Invalid request data.", "VALIDATION_ERROR").ToActionResult();

            var result = await _supabaseService.UpdateUserAsync(id, dto);
            return result.ToActionResult();
        }

    }
}
