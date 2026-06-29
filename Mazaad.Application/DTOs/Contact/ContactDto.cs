// Mazaad.Application/DTOs/Contact/ContactDto.cs

using System.ComponentModel.DataAnnotations;

namespace Mazaad.Application.DTOs.Contact
{
    public class ContactDto
    {
        [Required(ErrorMessage = "الاسم مطلوب")]
        [StringLength(100, ErrorMessage = "الاسم طويل جدًا")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "الإيميل مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة الإيميل غير صحيحة")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "الرسالة مطلوبة")]
        [StringLength(2000, ErrorMessage = "الرسالة طويلة جدًا")]
        public string Message { get; set; } = string.Empty;
    }
}