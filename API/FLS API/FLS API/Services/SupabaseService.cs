using FLS_API.DL.Models;
using FLS_API.DTOs;
using FLS_API.DL.DTOs;
using FLS_API.Models;
using Supabase;
using Supabase.Realtime;

namespace FLS_API.BL
{
    public class SupabaseService
    {
        private readonly Supabase.Client _client;

        public SupabaseService(Models.SupabaseOptions options)
        {
            if (string.IsNullOrEmpty(options.Url))
                throw new ArgumentException("Supabase URL is required", nameof(options));
            
            // Use AnonKey for PostgREST operations, ServiceRoleKey for admin operations
            var apiKey = !string.IsNullOrEmpty(options.AnonKey) ? options.AnonKey : options.ServiceRoleKey;
            
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentException("Supabase API key (AnonKey or ServiceRoleKey) is required", nameof(options));

            _client = new Supabase.Client(
                options.Url,
                apiKey
            );
            
            _client.InitializeAsync().GetAwaiter().GetResult();
        }

        public Supabase.Client Client => _client;

        public async Task<ServiceResult<UserResponseDTO>> SignUpUserAsync(string fullName, string email, string password)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(email))
                return ServiceResult<UserResponseDTO>.Failure("Email cannot be null or empty.", "INVALID_EMAIL");
            
            if (string.IsNullOrWhiteSpace(password))
                return ServiceResult<UserResponseDTO>.Failure("Password cannot be null or empty.", "INVALID_PASSWORD");
            
            if (string.IsNullOrWhiteSpace(fullName))
                return ServiceResult<UserResponseDTO>.Failure("Full name cannot be null or empty.", "INVALID_FULLNAME");

            try
            {
                // Sign up with Supabase Auth
                var authResult = await _client.Auth.SignUp(email, password);

                if (authResult.User == null)
                {
                    return ServiceResult<UserResponseDTO>.Failure("Unable to create Supabase Auth user.", "AUTH_CREATION_FAILED");
                }

                // Insert into users table
                var user = new User
                {
                    Id = authResult.User.Id,
                    FullName = fullName.Trim(),
                    Email = email.Trim().ToLowerInvariant(),
                    Role = "User",
                    CreatedAt = DateTime.UtcNow
                };
                
                try
                {
                    await _client.From<User>().Insert(user);
                }
                catch (Exception ex)
                {
                    return ServiceResult<UserResponseDTO>.Failure($"Failed to insert user into database: {ex.Message}", "DATABASE_INSERT_FAILED");
                }
                
                return ServiceResult<UserResponseDTO>.Success(new UserResponseDTO
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return ServiceResult<UserResponseDTO>.Failure(ex.Message, "SIGNUP_FAILED");
            }
        }

        public async Task<ServiceResult<UserResponseDTO>> SignInUserAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
                return ServiceResult<UserResponseDTO>.Failure("Email cannot be null or empty.", "INVALID_EMAIL");
            
            if (string.IsNullOrWhiteSpace(password))
                return ServiceResult<UserResponseDTO>.Failure("Password cannot be null or empty.", "INVALID_PASSWORD");

            try
            {
                var session = await _client.Auth.SignIn(email, password);
                if (session.User == null)
                {
                    return ServiceResult<UserResponseDTO>.Failure("Invalid email or password.", "INVALID_CREDENTIALS");
                }

                var user = await GetUserByEmailAsync(email);
                if (user == null)
                {
                    return ServiceResult<UserResponseDTO>.Failure("User not found in database.", "USER_NOT_FOUND");
                }

                return ServiceResult<UserResponseDTO>.Success(new UserResponseDTO
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return ServiceResult<UserResponseDTO>.Failure($"Sign in failed: {ex.Message}", "SIGNIN_FAILED");
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            try
            {
                var response = await _client.From<User>()
                    .Where(u => u.Email == email)
                    .Get();
                return response.Models.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<UserResponseDTO>> GetAllUsersAsync()
        {
            var response = await _client.From<User>().Get();
            return response.Models.Select(u => new UserResponseDTO
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                CreatedAt = u.CreatedAt
            }).ToList();
        }

        public async Task<UserResponseDTO?> GetUserByIdAsync(string id)
        {
            var user = await GetUserByIdAsync(id);
            if (user == null)
            {
                return null;
            }
            return new UserResponseDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<ServiceResult<UserResponseDTO>> UpdateUserAsync(string id, UpdateUserDTO dto)
        {
            if (string.IsNullOrWhiteSpace(id))
                return ServiceResult<UserResponseDTO>.Failure("User ID cannot be null or empty.", "INVALID_ID");

            try
            {
                var existing = await _client.From<User>().Where(u => u.Id == id).Get();
                var user = existing.Models.FirstOrDefault();
                if (user == null)
                {
                    return ServiceResult<UserResponseDTO>.Failure("User not found.", "USER_NOT_FOUND");
                }

                // Update FullName if provided
                if (!string.IsNullOrWhiteSpace(dto.FullName))
                {
                    user.FullName = dto.FullName.Trim();
                }

                // Update Role if provided - validate enum value
                if (!string.IsNullOrWhiteSpace(dto.Role))
                {
                    var roleLower = dto.Role.Trim().ToLowerInvariant();
                    // Validate against known enum values (adjust based on your actual enum)
                    if (roleLower == "user" || roleLower == "admin" || roleLower == "User" || roleLower == "Admin")
                    {
                        user.Role = dto.Role.Trim();
                    }
                    else
                    {
                        return ServiceResult<UserResponseDTO>.Failure($"Invalid role value. Must be 'user' or 'admin', got: {dto.Role}", "INVALID_ROLE");
                    }
                }

                await _client.From<User>().Update(user);
                
                return ServiceResult<UserResponseDTO>.Success(new UserResponseDTO
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return ServiceResult<UserResponseDTO>.Failure($"Update failed: {ex.Message}", "UPDATE_FAILED");
            }
        }
    }
}
