using System.ComponentModel.DataAnnotations;

namespace FLS_API.DL.DTOs
{
    public class SignInDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}

