// Mazaad.Domain/Enums/CompanyVerificationStatus.cs

namespace Mazaad.Domain.Enums
{
    public enum CompanyVerificationStatus
    {
        /// <summary>
        /// الشركة اتسجلت وبتستنى مراجعة الـ SuperAdmin.
        /// الـ default state لأي شركة جديدة.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// الـ SuperAdmin وافق — الشركة تقدر تشارك في الـ auctions.
        /// </summary>
        Verified = 1,

        /// <summary>
        /// الـ SuperAdmin رفض — الشركة مش هتقدر تشارك.
        /// لازم يكون فيه RejectionReason.
        /// </summary>
        Rejected = 2,

        /// <summary>
        /// كانت Verified وتم إيقافها بعدين.
        /// محتاجة RejectionReason كمان.
        /// </summary>
        Suspended = 3,
    }
}