// Mazaad.Infrastructure/Services/Auth/CompanyRegistrationService.cs

using Mazaad.Application.Common;
using Mazaad.Application.DTOs.Auth;
using Mazaad.Application.DTOs.Company;
using Mazaad.Application.Interfaces.Services;
using Mazaad.Domain.Enums;
using Mazaad.Domain.Models;
using Mazaad.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mazaad.Infrastructure.Services.Auth
{
    public class CompanyRegistrationService : ICompanyRegistrationService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtService _jwtService;
        private readonly ISecurityLogService _securityLog;
        private readonly ICompanyDocumentService _documentService;

        public CompanyRegistrationService(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IJwtService jwtService,
            ISecurityLogService securityLog,
            ICompanyDocumentService documentService)
        {
            _context = context;
            _userManager = userManager;
            _jwtService = jwtService;
            _securityLog = securityLog;
            _documentService = documentService;
        }

        // ── Register Company + First Admin ────────────────────────────────────
        public async Task<Result<AuthResponseDto>> RegisterCompanyAsync(
            RegisterCompanyDto dto,
            string ipAddress)
        {
            // تأكد إن الـ email مش موجود
            var existingUser = await _userManager.FindByEmailAsync(dto.AdminEmail);
            if (existingUser != null)
                return Result<AuthResponseDto>.Failure("Email already registered.");

            // تأكد إن الـ industry موجود
            var industry = await _context.IndustryTypes.FindAsync(dto.IndustryId);
            if (industry == null)
                return Result<AuthResponseDto>.Failure("Invalid industry.");

            // كل العملية في transaction واحدة
            // إما Company + User + Documents كلهم اتعملوا أو ولا حاجة
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. إنشاء الشركة
                var company = new Companies
                {
                    IndustryId = dto.IndustryId,
                    CompanyName = dto.CompanyName,
                    CommercialRegNum = dto.CommercialRegNum,
                    TaxRegistrationNum = dto.TaxRegistrationNum,
                    City = dto.City,
                    AddressDetails = dto.AddressDetails,
                    VerificationStatus = CompanyVerificationStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Companies.Add(company);
                await _context.SaveChangesAsync();

                // 2. إنشاء الـ Admin
                var admin = new ApplicationUser
                {
                    FullName = dto.AdminFullName,
                    Email = dto.AdminEmail,
                    UserName = dto.AdminEmail,
                    JobTitle = dto.AdminJobTitle,
                    CompanyId = company.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(admin, dto.AdminPassword);
                if (!createResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return Result<AuthResponseDto>.Failure(
                        createResult.Errors.Select(e => e.Description));
                }

                await _userManager.AddToRoleAsync(admin, "CompanyAdmin");

                // 3. رفع المستندات
                var commercialDoc = await _documentService.UploadAsync(
                    company.Id,
                    admin.Id,
                    dto.CommercialRegisterDocument,
                    CompanyDocumentType.CommercialRegister);

                if (!commercialDoc.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return Result<AuthResponseDto>.Failure(commercialDoc.Error!);
                }

                var taxDoc = await _documentService.UploadAsync(
                    company.Id,
                    admin.Id,
                    dto.TaxCardDocument,
                    CompanyDocumentType.TaxCard);

                if (!taxDoc.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return Result<AuthResponseDto>.Failure(taxDoc.Error!);
                }

                // المستندات الإضافية اختيارية
                if (dto.AdditionalDocuments != null)
                {
                    foreach (var doc in dto.AdditionalDocuments)
                    {
                        await _documentService.UploadAsync(
                            company.Id,
                            admin.Id,
                            doc,
                            CompanyDocumentType.Other);
                    }
                }

                await transaction.CommitAsync();

                // Log the events
                await _securityLog.LogAsync(
                    SecurityEventType.CompanyRegistered,
                    success: true,
                    ipAddress: ipAddress,
                    userId: admin.Id,
                    email: admin.Email,
                    details: $"Company: {company.CompanyName}");

                // نرجع auth response عشان الـ user يبدأ يستخدم الـ app
                // لكن الـ company لسه Pending
                var roles = await _userManager.GetRolesAsync(admin);
                var accessToken = await _jwtService.GenerateAccessTokenAsync(admin, roles);

                return Result<AuthResponseDto>.Success(new AuthResponseDto
                {
                    AccessToken = accessToken,
                    AccessTokenExpiry = DateTime.UtcNow.AddMinutes(15),
                    User = new UserInfoDto
                    {
                        Id = admin.Id,
                        FullName = admin.FullName,
                        Email = admin.Email!,
                        JobTitle = admin.JobTitle,
                        CompanyId = company.Id,
                        CompanyName = company.CompanyName,
                        Roles = roles
                    }
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result<AuthResponseDto>.Failure($"Registration failed: {ex.Message}");
            }
        }

        // ── Get Pending Companies ─────────────────────────────────────────────
        public async Task<IEnumerable<PendingCompanyDto>> GetPendingCompaniesAsync()
        {
            var companies = await _context.Companies
                .Include(c => c.Industry)
                .Include(c => c.Users)
                .Where(c => c.VerificationStatus == CompanyVerificationStatus.Pending)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return companies.Select(c =>
            {
                var admin = c.Users.FirstOrDefault();
                return new PendingCompanyDto
                {
                    Id = c.Id,
                    CompanyName = c.CompanyName,
                    CommercialRegNum = c.CommercialRegNum,
                    TaxRegistrationNum = c.TaxRegistrationNum,
                    City = c.City,
                    IndustryName = c.Industry.IndustryName,
                    AdminEmail = admin?.Email ?? "",
                    AdminName = admin?.FullName ?? "",
                    RegisteredAt = c.CreatedAt
                };
            });
        }

        // ── Verify or Reject Company ──────────────────────────────────────────
        public async Task<Result> VerifyCompanyAsync(
            int companyId,
            int adminUserId,
            VerifyCompanyDto dto,
            string ipAddress)
        {
            var company = await _context.Companies.FindAsync(companyId);
            if (company == null)
                return Result.Failure("Company not found.");

            if (company.VerificationStatus != CompanyVerificationStatus.Pending)
                return Result.Failure("Company is not in pending state.");

            if (!dto.Approved && string.IsNullOrWhiteSpace(dto.RejectionReason))
                return Result.Failure("Rejection reason is required.");

            company.VerificationStatus = dto.Approved
                ? CompanyVerificationStatus.Verified
                : CompanyVerificationStatus.Rejected;

            company.VerifiedByUserId = adminUserId;
            company.VerifiedAt = DateTime.UtcNow;
            company.RejectionReason = dto.Approved ? null : dto.RejectionReason;
            company.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _securityLog.LogAsync(
                dto.Approved
                    ? SecurityEventType.CompanyVerified
                    : SecurityEventType.CompanyRejected,
                success: true,
                ipAddress: ipAddress,
                userId: adminUserId,
                details: $"Company: {company.CompanyName}" +
                         (dto.Approved ? "" : $" | Reason: {dto.RejectionReason}"));

            return Result.Success();
        }
    }
}