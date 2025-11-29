using Microsoft.AspNetCore.Mvc;
using FLS_API.DTOs;

namespace FLS_API.Utilities
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public object? Data { get; set; }
        public string? Message { get; set; }
        public string? ErrorCode { get; set; }

        public static ApiResponse SuccessResponse(object? data = null, string? message = null)
        {
            return new ApiResponse
            {
                Success = true,
                Data = data,
                Message = message ?? "Operation completed successfully"
            };
        }

        public static ApiResponse ErrorResponse(string message, string? errorCode = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode
            };
        }
    }

    public static class ApiResponseExtensions
    {
        public static IActionResult ToActionResult(this ApiResponse response, int? successStatusCode = null)
        {
            if (response.Success)
            {
                var statusCode = successStatusCode ?? StatusCodes.Status200OK;
                return new ObjectResult(response)
                {
                    StatusCode = statusCode
                };
            }

            // Determine status code based on error code
            var errorStatusCode = response.ErrorCode switch
            {
                "INVALID_CREDENTIALS" => StatusCodes.Status401Unauthorized,
                "USER_NOT_FOUND" => StatusCodes.Status404NotFound,
                "INVALID_EMAIL" or "INVALID_PASSWORD" or "INVALID_FULLNAME" or "INVALID_ID" or "INVALID_ROLE" => StatusCodes.Status400BadRequest,
                "EMAIL_ALREADY_EXISTS" => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status400BadRequest
            };

            return new ObjectResult(response)
            {
                StatusCode = errorStatusCode
            };
        }

        public static IActionResult ToActionResult<T>(this ServiceResult<T> serviceResult, int? successStatusCode = null)
        {
            if (serviceResult.IsSuccess)
            {
                var response = ApiResponse.SuccessResponse(serviceResult.Data);
                return response.ToActionResult(successStatusCode);
            }

            var errorResponse = ApiResponse.ErrorResponse(serviceResult.ErrorMessage ?? "An error occurred", serviceResult.ErrorCode);
            return errorResponse.ToActionResult();
        }
    }
}