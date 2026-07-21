using Mazaad.Application.DTOs;
using Mazaad.Application.Interfaces.Services;
using Mazaad.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Mazaad.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ListingController : ControllerBase
    {
        private readonly IListingService _listingService;

        public ListingController(IListingService listingService)
        {
            _listingService = listingService;
        }

        /// <summary>
        /// Marketplace grid: returns paginated listing cards with optional filters.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ListingFilterDto filter)
        {
            var result = await _listingService.GetListingsAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Dashboard "My Listings": paginated listings owned by the authenticated company.
        /// </summary>
        [HttpGet("mine")]
        [Authorize]
        public async Task<IActionResult> GetMine([FromQuery] ListingFilterDto filter)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(new { error = "A valid company account is required." });

            var result = await _listingService.GetMyListingsAsync(companyId.Value, filter);
            return Ok(result);
        }

        /// <summary>Summary of a single listing.</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var listing = await _listingService.GetListingByIdAsync(id);
            if (listing == null) return NotFound();
            return Ok(listing);
        }

        /// <summary>
        /// Full bidding-room detail: includes company name, category name,
        /// technical specs, location, due-diligence URLs, and top 5 bids.
        /// </summary>
        [HttpGet("{id}/detail")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var detail = await _listingService.GetListingDetailAsync(id);
            if (detail == null) return NotFound();
            return Ok(detail);
        }

        /// <summary>Create a new listing. Requires CompanyAdmin or CompanyUser JWT token.</summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateListingDto request)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(new { error = "A valid company account is required to create listings." });

            try
            {
                var created = await _listingService.CreateListingAsync(companyId.Value, request);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred.", detail = ex.Message });
            }
        }

        /// <summary>Update a listing's mutable fields. Only members of the owning company can update.</summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] CreateListingDto request)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(new { error = "A valid company account is required to update listings." });

            try
            {
                var updated = await _listingService.UpdateListingAsync(id, companyId.Value, request);
                if (updated == null) return NotFound(new { error = "Listing not found or you do not own it." });
                return Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred.", detail = ex.Message });
            }
        }

        /// <summary>
        /// Cancel a listing — sets status to Cancelled without deleting data.
        /// Only members of the owning company can cancel.
        /// </summary>
        [HttpPatch("{id}/cancel")]
        [Authorize]
        public async Task<IActionResult> Cancel(int id)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(new { error = "A valid company account is required to cancel listings." });

            var success = await _listingService.CancelListingAsync(id, companyId.Value);
            if (!success) return BadRequest(new { error = "Could not cancel listing. It may not exist, already be cancelled, or you don't own it." });
            return NoContent();
        }

        /// <summary>
        /// End an auction immediately — sets status to Ended and EndDate to now,
        /// and returns the winning bid (if any) so the caller can announce it.
        /// Only members of the owning company can end it. Cannot end an already
        /// Ended or Cancelled auction.
        /// </summary>
        [HttpPatch("{id}/end")]
        [Authorize]
        public async Task<IActionResult> EndNow(int id)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(new { error = "A valid company account is required to end auctions." });

            var result = await _listingService.EndListingNowAsync(id, companyId.Value);
            if (result == null) return BadRequest(new { error = "Could not end auction. It may not exist, already be ended/cancelled, or you don't own it." });
            return Ok(result);
        }

        /// <summary>Soft-delete a listing. Only members of the owning company can delete.</summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(new { error = "A valid company account is required to delete listings." });

            var success = await _listingService.DeleteListingAsync(id, companyId.Value);
            if (!success) return BadRequest(new { error = "Could not delete listing. It may not exist or you don't own it." });
            return NoContent();
        }

        /// <summary>Upload or replace the primary image for a listing.</summary>
        [HttpPost("{id}/image")]
        [Authorize]
        public async Task<IActionResult> UploadImage(int id, IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest(new { error = "No image file provided." });

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(image.ContentType.ToLower()))
                return BadRequest(new { error = "Only JPEG, PNG, and WebP images are allowed." });

            if (image.Length > 5 * 1024 * 1024)
                return BadRequest(new { error = "Image size must not exceed 5MB." });

            var companyId = GetCurrentCompanyId();
            if (companyId == null)
                return Unauthorized(new { error = "A valid company account is required." });

            var updatedListing = await _listingService.UploadListingImageAsync(id, companyId.Value, image);
            if (updatedListing == null)
                return NotFound(new { error = "Listing not found or you do not own it." });

            return Ok(new { imageUrl = updatedListing.ImageUrl });
        }

        /// <summary>
        /// SuperAdmin queue: listings awaiting approval before they can go live.
        /// </summary>
        [HttpGet("admin/pending")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetPending()
        {
            var pending = await _listingService.GetPendingListingsAsync();
            return Ok(pending);
        }

        /// <summary>
        /// SuperAdmin approves or rejects a pending listing.
        /// Approving moves it to Upcoming/Active so it can go live; rejecting requires a reason.
        /// </summary>
        [HttpPatch("{id}/approve")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Approve(int id, [FromBody] ApproveListingDto dto)
        {
            var adminUserId = GetCurrentUserId();
            if (adminUserId == null)
                return Unauthorized(new { error = "A valid admin account is required." });

            var result = await _listingService.ApproveListingAsync(id, adminUserId.Value, dto, GetIpAddress());
            if (!result.Succeeded)
                return BadRequest(new { error = result.Error });

            return NoContent();
        }
        /// <summary>
        /// SuperAdmin/Admin endpoint: returns all listings without pagination.
        /// Filters by Status if provided; returns all statuses if null.
        /// </summary>
        [HttpGet("admin/all")]
        //[Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> GetAllAdmin([FromQuery] ListingStatus? status)
        {
            var listings = await _listingService.GetAllListingsAdminAsync(status);
            return Ok(listings);
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private int? GetCurrentCompanyId()
        {
            var claim = User.FindFirst("companyId")?.Value;
            if (string.IsNullOrWhiteSpace(claim)) return null;
            return int.TryParse(claim, out var id) && id > 0 ? id : null;
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst("uid")?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }

        private string GetIpAddress() =>
            Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded)
                ? forwarded.ToString()
                : HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}