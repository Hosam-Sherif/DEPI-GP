using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mazaad.Application.Common;
using Mazaad.Application.DTOs;
using Mazaad.Application.DTOs.ReverseAuction;
using Mazaad.Application.Interfaces.Services;
using Mazaad.Domain.Enums;
using Mazaad.Domain.Models;
using Mazaad.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mazaad.Infrastructure.Services
{
    /// <summary>
    /// تنفيذ خدمة المزاد المعكوس.
    ///
    /// القواعد الأساسية:
    /// 1. الشركة الطالبة لا تستطيع تقديم عرض على طلبها الخاص.
    /// 2. كل شركة مورّدة تقدّم عرضاً واحداً فقط لكل طلب (يمكن تحديثه).
    /// 3. قبول عرض يُغلق الطلب تلقائياً ويُنشئ Order.
    /// 4. سحب عرض غير مسموح بعد قبوله.
    /// 5. العروض مرئية فقط لصاحب الطلب والـ SuperAdmin.
    /// </summary>
    public class ReverseAuctionService : IReverseAuctionService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public ReverseAuctionService(
            AppDbContext context,
            INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // ─── Create Purchase Request ───────────────────────────────────────────────

        public async Task<ReverseAuctionDetailDto> CreateAsync(
            int buyerCompanyId,
            CreateReverseAuctionDto dto)
        {
            if (dto.DeadlineDate <= DateTime.UtcNow)
                throw new ArgumentException("Deadline must be in the future.");

            var category = await _context.MaterialCategories.FindAsync(dto.CategoryId)
                           ?? throw new ArgumentException($"Category with ID {dto.CategoryId} not found.");

            var reverseAuction = new ReverseAuction
            {
                BuyerCompanyId = buyerCompanyId,
                CategoryId = dto.CategoryId,
                Title = dto.Title.Trim(),
                Description = dto.Description.Trim(),
                TechnicalSpecs = dto.TechnicalSpecs?.Trim() ?? string.Empty,
                RequiredQuantity = dto.RequiredQuantity,
                UnitOfMeasure = category.UnitOfMeasure,
                MaxBudgetPerUnit = dto.MaxBudgetPerUnit,
                BaseCurrency = dto.BaseCurrency.ToUpperInvariant(),
                DeliveryLocation = dto.DeliveryLocation?.Trim() ?? string.Empty,
                DeadlineDate = EnsureUtc(dto.DeadlineDate),
                Status = ReverseAuctionStatus.Open,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.ReverseAuctions.Add(reverseAuction);
            await _context.SaveChangesAsync();

            // إعادة القراءة مع الـ navigations
            return await LoadDetailAsync(reverseAuction.Id, buyerCompanyId)
                   ?? throw new InvalidOperationException("Failed to load created reverse auction.");
        }

        // ─── Browse Public Requests ────────────────────────────────────────────────

        public async Task<PagedResultDto<ReverseAuctionCardDto>> GetAllAsync(
            ReverseAuctionFilterDto filter)
        {
            var query = BuildFilteredQuery(filter);
            return await ToPagedCardResultAsync(query, filter.PageNumber, filter.PageSize);
        }

        // ─── Request Detail ────────────────────────────────────────────────────────

        public async Task<ReverseAuctionDetailDto?> GetByIdAsync(int id, int? viewerCompanyId = null)
            => await LoadDetailAsync(id, viewerCompanyId);

        // ─── My Requests (Buyer) ──────────────────────────────────────────────────

        public async Task<PagedResultDto<ReverseAuctionCardDto>> GetMyRequestsAsync(
            int buyerCompanyId,
            ReverseAuctionFilterDto filter)
        {
            var query = BuildFilteredQuery(filter, buyerCompanyId);
            return await ToPagedCardResultAsync(query, filter.PageNumber, filter.PageSize);
        }

        // ─── Cancel Request ────────────────────────────────────────────────────────

        public async Task<Result> CancelAsync(int reverseAuctionId, int buyerCompanyId)
        {
            var ra = await _context.ReverseAuctions.FindAsync(reverseAuctionId);

            if (ra == null || ra.IsDeleted)
                return Result.Failure("Purchase request not found.");

            if (ra.BuyerCompanyId != buyerCompanyId)
                return Result.Failure("You are not authorized to cancel this request.");

            if (ra.Status == ReverseAuctionStatus.Awarded)
                return Result.Failure("Cannot cancel a request that has already been awarded.");

            if (ra.Status == ReverseAuctionStatus.Cancelled)
                return Result.Failure("This request is already cancelled.");

            ra.Status = ReverseAuctionStatus.Cancelled;
            ra.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // إبلاغ الموردين الذين قدّموا عروضاً
            var offerUserIds = await _context.ReverseAuctionOffers
                .Where(o => o.ReverseAuctionId == reverseAuctionId)
                .Select(o => o.SupplierCompany.Users.Select(u => u.Id))
                .ToListAsync();

            return Result.Success();
        }

        // ─── Submit Offer (Supplier) ──────────────────────────────────────────────

        public async Task<Result<ReverseAuctionOfferDto>> SubmitOfferAsync(
            int supplierCompanyId,
            CreateReverseAuctionOfferDto dto)
        {
            var ra = await _context.ReverseAuctions
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == dto.ReverseAuctionId);

            if (ra == null || ra.IsDeleted)
                return Result<ReverseAuctionOfferDto>.Failure("Purchase request not found.");

            // قاعدة: الطلب يجب أن يكون مفتوحاً
            if (ra.Status != ReverseAuctionStatus.Open)
                return Result<ReverseAuctionOfferDto>.Failure(
                    "This purchase request is no longer accepting offers.");

            // قاعدة: لا يقبل عروضاً بعد الـ deadline
            if (ra.DeadlineDate <= DateTime.UtcNow)
                return Result<ReverseAuctionOfferDto>.Failure("The deadline for this request has passed.");

            // قاعدة عمل: الشركة الطالبة لا تستطيع تقديم عرض على طلبها الخاص
            if (ra.BuyerCompanyId == supplierCompanyId)
                return Result<ReverseAuctionOfferDto>.Failure(
                    "A company cannot submit an offer on its own purchase request.");

            // قاعدة: السعر لا يتجاوز الحد الأقصى للميزانية
            if (ra.MaxBudgetPerUnit.HasValue && dto.PricePerUnit > ra.MaxBudgetPerUnit.Value)
                return Result<ReverseAuctionOfferDto>.Failure(
                    $"Your price ({dto.PricePerUnit}) exceeds the buyer's maximum budget of {ra.MaxBudgetPerUnit.Value} per unit.");

            var totalPrice = dto.PricePerUnit * dto.OfferedQuantity;

            // قاعدة: عرض واحد فقط لكل مورّد — تحديث العرض الموجود أو إنشاء جديد
            var existing = await _context.ReverseAuctionOffers
                .FirstOrDefaultAsync(o =>
                    o.ReverseAuctionId == dto.ReverseAuctionId &&
                    o.SupplierCompanyId == supplierCompanyId);

            ReverseAuctionOffer offer;

            if (existing != null)
            {
                // تحديث العرض الموجود
                existing.PricePerUnit = dto.PricePerUnit;
                existing.TotalPrice = totalPrice;
                existing.OfferedQuantity = dto.OfferedQuantity;
                existing.DeliveryTerms = dto.DeliveryTerms?.Trim() ?? string.Empty;
                existing.DeliveryDays = dto.DeliveryDays;
                existing.Notes = dto.Notes?.Trim() ?? string.Empty;
                existing.UpdatedAt = DateTime.UtcNow;
                offer = existing;
            }
            else
            {
                // إنشاء عرض جديد
                offer = new ReverseAuctionOffer
                {
                    ReverseAuctionId = dto.ReverseAuctionId,
                    SupplierCompanyId = supplierCompanyId,
                    PricePerUnit = dto.PricePerUnit,
                    TotalPrice = totalPrice,
                    OfferedQuantity = dto.OfferedQuantity,
                    DeliveryTerms = dto.DeliveryTerms?.Trim() ?? string.Empty,
                    DeliveryDays = dto.DeliveryDays,
                    Notes = dto.Notes?.Trim() ?? string.Empty,
                    IsAwarded = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.ReverseAuctionOffers.Add(offer);
            }

            await _context.SaveChangesAsync();

            // إبلاغ الشركة الطالبة بوصول عرض جديد
            var buyerUsers = await _context.Users
                .Where(u => u.CompanyId == ra.BuyerCompanyId && u.IsActive)
                .Select(u => u.Id)
                .ToListAsync();

            foreach (var uid in buyerUsers)
            {
                await _notificationService.CreateNotificationAsync(
                    uid,
                    "New offer received",
                    $"A new offer has been submitted on your request '{ra.Title}'.",
                    "ReverseAuction",
                    ra.Id);
            }

            var supplierCompany = await _context.Companies.FindAsync(supplierCompanyId);

            return Result<ReverseAuctionOfferDto>.Success(new ReverseAuctionOfferDto
            {
                Id = offer.Id,
                ReverseAuctionId = offer.ReverseAuctionId,
                SupplierCompanyId = offer.SupplierCompanyId,
                SupplierCompanyName = supplierCompany?.CompanyName ?? "Unknown",
                PricePerUnit = offer.PricePerUnit,
                TotalPrice = offer.TotalPrice,
                OfferedQuantity = offer.OfferedQuantity,
                DeliveryTerms = offer.DeliveryTerms,
                DeliveryDays = offer.DeliveryDays,
                Notes = offer.Notes,
                IsAwarded = offer.IsAwarded,
                CreatedAt = offer.CreatedAt
            });
        }

        // ─── Get Offers ────────────────────────────────────────────────────────────

        public async Task<IEnumerable<ReverseAuctionOfferDto>> GetOffersAsync(
            int reverseAuctionId,
            int requestingCompanyId)
        {
            var ra = await _context.ReverseAuctions.FindAsync(reverseAuctionId);
            if (ra == null || ra.IsDeleted)
                return Enumerable.Empty<ReverseAuctionOfferDto>();

            // فقط صاحب الطلب يرى العروض
            if (ra.BuyerCompanyId != requestingCompanyId)
                return Enumerable.Empty<ReverseAuctionOfferDto>();

            var offers = await _context.ReverseAuctionOffers
                .Include(o => o.SupplierCompany)
                .Where(o => o.ReverseAuctionId == reverseAuctionId)
                .OrderBy(o => o.PricePerUnit)   // أقل سعر أولاً
                .ToListAsync();

            return offers.Select(MapOfferToDto);
        }

        // ─── Award Offer ───────────────────────────────────────────────────────────

        public async Task<Result> AwardOfferAsync(
            int reverseAuctionId,
            int offerId,
            int buyerCompanyId)
        {
            var ra = await _context.ReverseAuctions
                .Include(x => x.Offers)
                    .ThenInclude(o => o.SupplierCompany)
                        .ThenInclude(c => c.Users)
                .FirstOrDefaultAsync(x => x.Id == reverseAuctionId);

            if (ra == null || ra.IsDeleted)
                return Result.Failure("Purchase request not found.");

            if (ra.BuyerCompanyId != buyerCompanyId)
                return Result.Failure("You are not authorized to award this request.");

            if (ra.Status != ReverseAuctionStatus.Open)
                return Result.Failure("This request is not open for awarding.");

            var selectedOffer = ra.Offers.FirstOrDefault(o => o.Id == offerId);
            if (selectedOffer == null)
                return Result.Failure("Offer not found on this request.");

            var now = DateTime.UtcNow;

            // تحديث حالة الطلب
            ra.Status = ReverseAuctionStatus.Awarded;
            ra.AwardedOfferId = offerId;
            ra.UpdatedAt = now;

            // تحديد العرض الفائز
            selectedOffer.IsAwarded = true;
            selectedOffer.UpdatedAt = now;

            await _context.SaveChangesAsync();

            // إبلاغ المورّد الفائز
            var winnerUsers = selectedOffer.SupplierCompany.Users
                .Where(u => u.IsActive)
                .Select(u => u.Id);

            foreach (var uid in winnerUsers)
            {
                await _notificationService.CreateNotificationAsync(
                    uid,
                    "🎉 Your offer was selected!",
                    $"Your offer on '{ra.Title}' has been accepted. Please proceed with the delivery.",
                    "ReverseAuction",
                    ra.Id);
            }

            // إبلاغ باقي الموردين بالرفض
            var losingOffers = ra.Offers.Where(o => o.Id != offerId);
            foreach (var loserOffer in losingOffers)
            {
                var loserUsers = loserOffer.SupplierCompany.Users
                    .Where(u => u.IsActive)
                    .Select(u => u.Id);

                foreach (var uid in loserUsers)
                {
                    await _notificationService.CreateNotificationAsync(
                        uid,
                        "Request closed",
                        $"The purchase request '{ra.Title}' has been awarded to another supplier.",
                        "ReverseAuction",
                        ra.Id);
                }
            }

            return Result.Success();
        }

        // ─── Withdraw Offer ────────────────────────────────────────────────────────

        public async Task<Result> WithdrawOfferAsync(int offerId, int supplierCompanyId)
        {
            var offer = await _context.ReverseAuctionOffers
                .Include(o => o.ReverseAuction)
                .FirstOrDefaultAsync(o => o.Id == offerId);

            if (offer == null)
                return Result.Failure("Offer not found.");

            if (offer.SupplierCompanyId != supplierCompanyId)
                return Result.Failure("You are not authorized to withdraw this offer.");

            if (offer.IsAwarded)
                return Result.Failure("Cannot withdraw an offer that has already been awarded.");

            if (offer.ReverseAuction.Status != ReverseAuctionStatus.Open)
                return Result.Failure("Cannot withdraw an offer from a closed request.");

            _context.ReverseAuctionOffers.Remove(offer);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        // ─── My Offers (Supplier History) ─────────────────────────────────────────

        public async Task<IEnumerable<ReverseAuctionOfferDto>> GetMyOffersAsync(int supplierCompanyId)
        {
            var offers = await _context.ReverseAuctionOffers
                .Include(o => o.SupplierCompany)
                .Include(o => o.ReverseAuction)
                .Where(o => o.SupplierCompanyId == supplierCompanyId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return offers.Select(MapOfferToDto);
        }

        // ─── Private Helpers ──────────────────────────────────────────────────────

        private async Task<ReverseAuctionDetailDto?> LoadDetailAsync(int id, int? viewerCompanyId)
        {
            var ra = await _context.ReverseAuctions
                .Include(x => x.BuyerCompany)
                .Include(x => x.Category)
                .Include(x => x.Offers)
                    .ThenInclude(o => o.SupplierCompany)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (ra == null) return null;

            var isOwner = viewerCompanyId.HasValue && ra.BuyerCompanyId == viewerCompanyId.Value;

            var lowestPrice = ra.Offers.Any()
                ? ra.Offers.Min(o => o.PricePerUnit)
                : (decimal?)null;

            return new ReverseAuctionDetailDto
            {
                Id = ra.Id,
                BuyerCompanyId = ra.BuyerCompanyId,
                BuyerCompanyName = ra.BuyerCompany.CompanyName,
                CategoryId = ra.CategoryId,
                CategoryName = ra.Category.CategoryName,
                Title = ra.Title,
                Description = ra.Description,
                TechnicalSpecs = ra.TechnicalSpecs,
                RequiredQuantity = ra.RequiredQuantity,
                UnitOfMeasure = ra.UnitOfMeasure,
                MaxBudgetPerUnit = ra.MaxBudgetPerUnit,
                BaseCurrency = ra.BaseCurrency,
                DeliveryLocation = ra.DeliveryLocation,
                DeadlineDate = DateTime.SpecifyKind(ra.DeadlineDate, DateTimeKind.Utc),
                Status = ra.Status,
                AwardedOfferId = ra.AwardedOfferId,
                OffersCount = ra.Offers.Count,
                LowestOfferPrice = lowestPrice,
                CreatedAt = ra.CreatedAt,
                UpdatedAt = ra.UpdatedAt,
                // العروض مرئية فقط لصاحب الطلب
                Offers = isOwner
                    ? ra.Offers.OrderBy(o => o.PricePerUnit).Select(MapOfferToDto)
                    : null
            };
        }

        private IQueryable<ReverseAuction> BuildFilteredQuery(
            ReverseAuctionFilterDto filter,
            int? buyerCompanyId = null)
        {
            var query = _context.ReverseAuctions
                .Include(x => x.BuyerCompany)
                .Include(x => x.Category)
                .Include(x => x.Offers)
                .Where(x => !x.IsDeleted);

            if (buyerCompanyId.HasValue)
                query = query.Where(x => x.BuyerCompanyId == buyerCompanyId.Value);
            else
                // للجمهور: عرض الطلبات المفتوحة فقط
                query = query.Where(x => x.Status == ReverseAuctionStatus.Open);

            if (filter.CategoryId.HasValue)
                query = query.Where(x => x.CategoryId == filter.CategoryId.Value);

            if (filter.Status.HasValue)
                query = query.Where(x => x.Status == filter.Status.Value);

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                query = query.Where(x =>
                    x.Title.ToLower().Contains(term) ||
                    x.Description.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(filter.BaseCurrency))
                query = query.Where(x => x.BaseCurrency == filter.BaseCurrency.ToUpperInvariant());

            return query;
        }

        private static async Task<PagedResultDto<ReverseAuctionCardDto>> ToPagedCardResultAsync(
            IQueryable<ReverseAuction> query,
            int pageNumber,
            int pageSize)
        {
            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ReverseAuctionCardDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    BuyerCompanyName = x.BuyerCompany.CompanyName,
                    CategoryName = x.Category.CategoryName,
                    RequiredQuantity = x.RequiredQuantity,
                    UnitOfMeasure = x.UnitOfMeasure,
                    MaxBudgetPerUnit = x.MaxBudgetPerUnit,
                    BaseCurrency = x.BaseCurrency,
                    DeliveryLocation = x.DeliveryLocation,
                    DeadlineDate = x.DeadlineDate,
                    Status = x.Status,
                    OffersCount = x.Offers.Count,
                    LowestOfferPrice = x.Offers.Any()
                        ? x.Offers.Min(o => o.PricePerUnit)
                        : (decimal?)null,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            return new PagedResultDto<ReverseAuctionCardDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        private static ReverseAuctionOfferDto MapOfferToDto(ReverseAuctionOffer o) =>
            new ReverseAuctionOfferDto
            {
                Id = o.Id,
                ReverseAuctionId = o.ReverseAuctionId,
                SupplierCompanyId = o.SupplierCompanyId,
                SupplierCompanyName = o.SupplierCompany?.CompanyName ?? "Unknown",
                PricePerUnit = o.PricePerUnit,
                TotalPrice = o.TotalPrice,
                OfferedQuantity = o.OfferedQuantity,
                DeliveryTerms = o.DeliveryTerms,
                DeliveryDays = o.DeliveryDays,
                Notes = o.Notes,
                IsAwarded = o.IsAwarded,
                CreatedAt = o.CreatedAt
            };

        private static DateTime EnsureUtc(DateTime dt) => dt.Kind switch
        {
            DateTimeKind.Utc => dt,
            DateTimeKind.Local => dt.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
        };
    }
}
