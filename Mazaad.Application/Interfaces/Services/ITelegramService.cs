using Microsoft.AspNetCore.Http;

namespace Application.Interfaces
{
    public interface ITelegramService
    {
        Task<bool> SendReportAsync(string message, List<IFormFile> images);
    }
}