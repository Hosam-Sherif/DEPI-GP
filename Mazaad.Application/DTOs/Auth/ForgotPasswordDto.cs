// Mazaad.Application/DTOs/Auth/ForgotPasswordDto.cs

using System.ComponentModel.DataAnnotations;

namespace Mazaad.Application.DTOs.Auth
{
    public class ForgotPasswordDto
    {
        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = string.Empty;
    }
}