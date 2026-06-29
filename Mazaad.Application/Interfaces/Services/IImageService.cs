namespace Mazaad.Application.Interfaces.Services
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(Stream imageStream, string fileName, string folder);
        Task<bool> DeleteImageAsync(string publicId);
    }
}