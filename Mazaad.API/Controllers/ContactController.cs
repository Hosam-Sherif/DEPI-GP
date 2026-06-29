// Mazaad.API/Controllers/ContactController.cs

using Mazaad.Application.DTOs.Contact;
using Mazaad.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Mazaad.API.Controllers
{
    [ApiController]
    [Route("api/contact")]
    public class ContactController : ControllerBase
    {
        private readonly IContactService _contactService;

        public ContactController(IContactService contactService)
        {
            _contactService = contactService;
        }

        /// <summary>
        /// استقبال رسالة من صفحة "تواصل معنا" وإرسالها على إيميل الدعم.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Send([FromBody] ContactDto dto)
        {
            var result = await _contactService.SendContactMessageAsync(dto);

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            return Ok(new { message = "تم إرسال رسالتك بنجاح، سنتواصل معك قريبًا." });
        }
    }
}