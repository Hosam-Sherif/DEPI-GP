// Mazaad.Application/DTOs/Company/CompanyDocumentResponseDto.cs

using Mazaad.Domain.Enums;

namespace Mazaad.Application.DTOs.Company
{
    // بيترجع في قائمة المستندات — بدون FilePath عشان مش بنكشف المسار
    public class CompanyDocumentResponseDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public string UploadedByName { get; set; } = string.Empty;
    }
}