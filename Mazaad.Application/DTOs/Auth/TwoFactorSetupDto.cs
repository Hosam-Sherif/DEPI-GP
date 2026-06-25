// Mazaad.Application/DTOs/Auth/TwoFactorDto.cs

namespace Mazaad.Application.DTOs.Auth
{
    // بيترجع للـ user لما يفعّل الـ 2FA
    public class TwoFactorSetupDto
    {
        // الـ QR Code كـ base64 image للـ authenticator app
        public string QrCodeBase64 { get; set; } = string.Empty;

        // الـ manual key لو حد مش قادر يـ scan الـ QR
        public string ManualEntryKey { get; set; } = string.Empty;
    }

    // اللي بيبعته الـ user عشان يكمل الـ 2FA login
    public class TwoFactorVerifyDto
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    // لتفعيل أو إلغاء الـ 2FA
    public class TwoFactorToggleDto
    {
        // الـ code من الـ authenticator app عشان نتأكد إن الـ user فعلاً عنده الـ app
        public string VerificationCode { get; set; } = string.Empty;
    }
}