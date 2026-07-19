using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupportController : ControllerBase
    {
        private readonly ITelegramService _telegramService;

        public SupportController(ITelegramService telegramService)
        {
            _telegramService = telegramService;
        }

        [HttpPost("submit-report")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SubmitReport([FromForm] SubmitReportDto dto)
        {
            // بنمرر البيانات من الـ DTO للسيرفيس مباشرة
            var result = await _telegramService.SendReportAsync(dto.Message, dto.Images);
            return result ? Ok("تم إرسال التقرير بنجاح") : BadRequest("فشل في الإرسال");
        }
    }
}
