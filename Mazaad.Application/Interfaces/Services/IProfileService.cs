// Mazaad.Application/Interfaces/Services/IProfileService.cs

using Mazaad.Application.Common;
using Mazaad.Application.DTOs.Auth;

namespace Mazaad.Application.Interfaces.Services
{
    public interface IProfileService
    {
        Task<Result<MyProfileDto>> GetMyProfileAsync(int userId);

        Task<Result<MyProfileDto>> UpdateMyProfileAsync(
            int userId,
            UpdateProfileDto dto,
            string ipAddress);
    }
}