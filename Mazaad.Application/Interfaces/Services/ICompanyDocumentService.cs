// Mazaad.Application/Interfaces/Services/ICompanyDocumentService.cs

using Mazaad.Application.Common;
using Mazaad.Application.DTOs.Company;
using Mazaad.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Mazaad.Application.Interfaces.Services
{
    public interface ICompanyDocumentService
    {
        /// <summary>
        /// يرفع مستند واحد لشركة معينة.
        /// بيتكال من ICompanyRegistrationService أثناء التسجيل.
        /// </summary>
        Task<Result<CompanyDocumentResponseDto>> UploadAsync(
            int companyId,
            int uploadedByUserId,
            IFormFile file,
            CompanyDocumentType documentType);

        /// <summary>
        /// يجيب كل مستندات شركة معينة.
        /// بيتكال من SuperAdmin في صفحة الـ verification.
        /// </summary>
        Task<IEnumerable<CompanyDocumentResponseDto>> GetCompanyDocumentsAsync(int companyId);

        /// <summary>
        /// يجيب الـ file stream لتحميل المستند.
        /// SuperAdmin بس يقدر يحمّل.
        /// </summary>
        Task<Result<(Stream FileStream, string ContentType, string FileName)>> DownloadAsync(
            int documentId,
            int requestingUserId);

        /// <summary>
        /// يحذف مستند — CompanyAdmin بس قبل الـ verification.
        /// </summary>
        Task<Result> DeleteAsync(int documentId, int requestingUserId);
    }
}