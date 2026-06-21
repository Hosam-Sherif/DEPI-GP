// Mazaad.Infrastructure/Services/Auth/CompanyDocumentService.cs

using Mazaad.Application.Common;
using Mazaad.Application.DTOs.Company;
using Mazaad.Application.Interfaces.Services;
using Mazaad.Domain.Enums;
using Mazaad.Domain.Models;
using Mazaad.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Mazaad.Infrastructure.Services.Auth
{
    public class CompanyDocumentService : ICompanyDocumentService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        private static readonly string[] AllowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

        public CompanyDocumentService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ── Upload ────────────────────────────────────────────────────────────
        public async Task<Result<CompanyDocumentResponseDto>> UploadAsync(
            int companyId,
            int uploadedByUserId,
            IFormFile file,
            CompanyDocumentType documentType)
        {
            // Validate
            var validationError = ValidateFile(file);
            if (validationError != null)
                return Result<CompanyDocumentResponseDto>.Failure(validationError);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var storedName = $"{Guid.NewGuid()}{extension}";
            var uploadFolder = GetUploadPath(companyId);

            Directory.CreateDirectory(uploadFolder);

            var fullPath = Path.Combine(uploadFolder, storedName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var document = new CompanyDocument
            {
                CompanyId = companyId,
                DocumentType = documentType,
                OriginalFileName = file.FileName,
                StoredFileName = storedName,
                FilePath = fullPath,
                FileSizeBytes = file.Length,
                ContentType = file.ContentType,
                UploadedAt = DateTime.UtcNow,
                UploadedByUserId = uploadedByUserId
            };

            _context.CompanyDocuments.Add(document);
            await _context.SaveChangesAsync();

            return Result<CompanyDocumentResponseDto>.Success(MapToDto(document, ""));
        }

        // ── Get Documents ─────────────────────────────────────────────────────
        public async Task<IEnumerable<CompanyDocumentResponseDto>> GetCompanyDocumentsAsync(
            int companyId)
        {
            var docs = await _context.CompanyDocuments
                .Include(d => d.UploadedByUser)
                .Where(d => d.CompanyId == companyId)
                .OrderBy(d => d.DocumentType)
                .ToListAsync();

            return docs.Select(d => MapToDto(d, d.UploadedByUser?.FullName ?? ""));
        }

        // ── Download ──────────────────────────────────────────────────────────
        public async Task<Result<(Stream FileStream, string ContentType, string FileName)>>
            DownloadAsync(int documentId, int requestingUserId)
        {
            var doc = await _context.CompanyDocuments.FindAsync(documentId);
            if (doc == null)
                return Result<(Stream, string, string)>.Failure("Document not found.");

            if (!File.Exists(doc.FilePath))
                return Result<(Stream, string, string)>.Failure("File not found on disk.");

            var stream = new FileStream(doc.FilePath, FileMode.Open, FileAccess.Read);
            return Result<(Stream, string, string)>.Success(
                (stream, doc.ContentType, doc.OriginalFileName));
        }

        // ── Delete ────────────────────────────────────────────────────────────
        public async Task<Result> DeleteAsync(int documentId, int requestingUserId)
        {
            var doc = await _context.CompanyDocuments
                .Include(d => d.Company)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (doc == null)
                return Result.Failure("Document not found.");

            // بس قبل الـ verification
            if (doc.Company.VerificationStatus != CompanyVerificationStatus.Pending)
                return Result.Failure("Cannot delete documents after verification process started.");

            if (File.Exists(doc.FilePath))
                File.Delete(doc.FilePath);

            _context.CompanyDocuments.Remove(doc);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private string GetUploadPath(int companyId)
        {
            var basePath = _config["FileStorage:CompanyDocumentsPath"]
                           ?? Path.Combine("uploads", "company-documents");
            return Path.Combine(basePath, companyId.ToString());
        }

        private static string? ValidateFile(IFormFile file)
        {
            if (file.Length == 0)
                return "File is empty.";

            if (file.Length > MaxFileSizeBytes)
                return "File size exceeds 10MB limit.";

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                return $"File type not allowed. Allowed: {string.Join(", ", AllowedExtensions)}";

            return null;
        }

        private static CompanyDocumentResponseDto MapToDto(
            CompanyDocument doc,
            string uploaderName) => new()
            {
                Id = doc.Id,
                CompanyId = doc.CompanyId,
                DocumentType = doc.DocumentType.ToString(),
                OriginalFileName = doc.OriginalFileName,
                FileSizeBytes = doc.FileSizeBytes,
                ContentType = doc.ContentType,
                UploadedAt = doc.UploadedAt,
                UploadedByName = uploaderName
            };
    }
}