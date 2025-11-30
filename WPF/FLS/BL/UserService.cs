using System;
using System.Threading.Tasks;
using FLS.DL;
using FLS.Models;

namespace FLS.BL
{
    public class UserService
    {
        private readonly UserApiClient _userApiClient;

        public UserService(UserApiClient userApiClient)
        {
            _userApiClient = userApiClient ?? throw new ArgumentNullException(nameof(userApiClient));
        }

        public async Task<ApiResponse<UserResponse>> SignInAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = "Email is required",
                    ErrorCode = "INVALID_EMAIL"
                };
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = "Password is required",
                    ErrorCode = "INVALID_PASSWORD"
                };
            }

            var request = new SignInRequest
            {
                Email = email.Trim().ToLower(),
                Password = password
            };

            return await _userApiClient.SignInAsync(request);
        }

        public async Task<ApiResponse<UserResponse>> SignUpAsync(string fullName, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = "Full name is required",
                    ErrorCode = "INVALID_FULLNAME"
                };
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = "Email is required",
                    ErrorCode = "INVALID_EMAIL"
                };
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = "Password is required",
                    ErrorCode = "INVALID_PASSWORD"
                };
            }

            var request = new SignUpRequest
            {
                FullName = fullName.Trim(),
                Email = email.Trim().ToLower(),
                Password = password
            };

            return await _userApiClient.SignUpAsync(request);
        }
    }
}

