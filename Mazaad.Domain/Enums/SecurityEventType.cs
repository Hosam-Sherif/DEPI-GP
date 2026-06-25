namespace Mazaad.Domain.Enums
{
    public enum SecurityEventType
    {
        // ── Authentication ──────────────────────────────
        LoginSuccess = 1,
        LoginFailed = 2,
        Logout = 3,
        TokenRefreshed = 4,
        TokenRevoked = 5,

        // ── Account lifecycle ───────────────────────────
        AccountRegistered = 10,
        AccountActivated = 11,
        AccountDeactivated = 12,
        AccountLockedOut = 13,
        AccountUnlocked = 14,

        // ── Password ────────────────────────────────────
        PasswordChanged = 20,
        PasswordResetRequested = 21,
        PasswordResetCompleted = 22,

        // ── Two-Factor Authentication ───────────────────
        TwoFactorEnabled = 30,
        TwoFactorDisabled = 31,
        TwoFactorLoginSuccess = 32,
        TwoFactorLoginFailed = 33,
        TwoFactorCodeGenerated = 34,

        // ── Roles & Permissions ─────────────────────────
        RoleAssigned = 40,
        RoleRevoked = 41,

        // ── Company ─────────────────────────────────────
        CompanyRegistered = 50,
        CompanyVerified = 51,
        CompanyRejected = 52,
        UserAddedToCompany = 53,
        UserRemovedFromCompany = 54,
        UserRoleChangedInCompany = 55,
    }
}