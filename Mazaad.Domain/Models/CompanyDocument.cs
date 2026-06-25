// Mazaad.Domain/Models/CompanyDocument.cs

using Mazaad.Domain.Enums;

namespace Mazaad.Domain.Models
{
    /// <summary>
    /// مستند مرفوع من الشركة للـ verification.
    /// بيتخزن على الـ file system والـ DB بيحتفظ بالـ metadata بس.
    /// </summary>
    public class CompanyDocument
    {
        public int Id { get; set; }

        public int CompanyId { get; set; }

        /// <summary>نوع المستند: سجل تجاري، بطاقة ضريبية، إلخ.</summary>
        public CompanyDocumentType DocumentType { get; set; }

        /// <summary>الاسم الأصلي للملف اللي رفعه الـ user.</summary>
        public string OriginalFileName { get; set; } = string.Empty;

        /// <summary>
        /// الاسم المحفوظ على الـ disk — GUID عشان نتجنب conflicts
        /// ومنعرفش الاسم الحقيقي من الـ URL.
        /// </summary>
        public string StoredFileName { get; set; } = string.Empty;

        /// <summary>المسار الكامل على الـ server.</summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>حجم الملف بالـ bytes.</summary>
        public long FileSizeBytes { get; set; }

        /// <summary>pdf, jpg, png, إلخ.</summary>
        public string ContentType { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        /// <summary>الـ user اللي رفع المستند.</summary>
        public int UploadedByUserId { get; set; }

        // Navigation
        public Companies Company { get; set; } = null!;
        public ApplicationUser UploadedByUser { get; set; } = null!;
    }
}