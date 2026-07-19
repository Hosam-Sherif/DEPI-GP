// Mazaad.Application/DTOs/ListingApprovalDto.cs

using System.ComponentModel.DataAnnotations;

namespace Mazaad.Application.DTOs
{
    public class ApproveListingDto
    {
        [Required]
        public bool Approved { get; set; }

        // مطلوب لو Approved = false
        public string? RejectionReason { get; set; }
    }

    // بيترجع للـ SuperAdmin في قائمة المزادات المنتظرة للموافقة
    public class PendingListingDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal StartingPrice { get; set; }
        public string BaseCurrency { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}