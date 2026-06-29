using Mazaad.Domain.Models;
using Mazaad.Domain.Enums;
using Mazaad.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Mazaad.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxFileSize = 5 * 1024 * 1024;
        private const string SuperAdminRole = "SuperAdmin";

        public InventoryController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ── Current User / Ownership Helpers ────────────────────────────────

        private int? GetCurrentCompanyId()
        {
            var claim = User.FindFirst("companyId")?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }

        private bool IsSuperAdmin() => User.IsInRole(SuperAdminRole);

        private bool CanAccessCompany(int companyId)
        {
            if (IsSuperAdmin()) return true;
            var currentCompanyId = GetCurrentCompanyId();
            return currentCompanyId.HasValue && currentCompanyId.Value == companyId;
        }

        // ── Endpoints ─────────────────────────────────────────────────────────

        [HttpGet("company/{companyId}")]
        public async Task<IActionResult> GetCompanyInventory(
            int companyId,
            [FromQuery] InventoryItemStatus? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (!CanAccessCompany(companyId)) return Forbid();

            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var query = _context.InventoryItems
                .Include(i => i.Category)
                .Where(i => i.company_id == companyId);

            if (status.HasValue)
                query = query.Where(i => i.status == status.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(i => i.created_at)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new
                {
                    i.Id,
                    i.name,
                    i.description,
                    i.quantity,
                    i.unit_of_measure,
                    i.minimum_auction_price,
                    i.current_market_price,
                    i.status,
                    i.image_name,
                    imageUrl = i.image_path != null ? $"/api/inventory/image/{i.Id}" : null,
                    categoryName = i.Category.CategoryName,
                    category_id = i.category_id,
                    i.created_at,
                    i.updated_at
                })
                .ToListAsync();

            return Ok(new
            {
                company_id = companyId,
                total_count = totalCount,
                page,
                page_size = pageSize,
                items
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetItem(int id)
        {
            var item = await _context.InventoryItems
                .Include(i => i.Category)
                .Include(i => i.Company)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
                return NotFound(new { message = "Item not found." });

            if (!CanAccessCompany(item.company_id))
                return Forbid();

            return Ok(new
            {
                item.Id,
                item.name,
                item.description,
                item.quantity,
                item.unit_of_measure,
                item.minimum_auction_price,
                item.current_market_price,
                item.status,
                item.image_name,
                imageUrl = item.image_path != null ? $"/api/inventory/image/{item.Id}" : null,
                categoryName = item.Category.CategoryName,
                category_id = item.category_id,
                companyName = item.Company.CompanyName,
                item.created_at,
                item.updated_at
            });
        }

        [HttpPost]
        [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
        public async Task<IActionResult> CreateItem([FromForm] CreateInventoryItemDto dto)
        {
            int targetCompanyId;

            if (IsSuperAdmin())
            {
                targetCompanyId = dto.company_id;
            }
            else
            {
                var currentCompanyId = GetCurrentCompanyId();
                if (!currentCompanyId.HasValue) return Forbid();
                targetCompanyId = currentCompanyId.Value;
            }

            var company = await _context.Companies.FindAsync(targetCompanyId);
            if (company == null)
                return NotFound(new { message = "Company not found." });

            var category = await _context.MaterialCategories.FindAsync(dto.category_id);
            if (category == null)
                return NotFound(new { message = "Category not found." });

            string? imagePath = null;
            string? imageName = null;

            if (dto.image != null)
            {
                var validationResult = ValidateImage(dto.image);
                if (validationResult != null)
                    return BadRequest(new { message = validationResult });

                var uploadResult = await SaveImage(dto.image, targetCompanyId);
                imagePath = uploadResult.path;
                imageName = uploadResult.name;
            }

            var item = new InventoryItem
            {
                company_id = targetCompanyId,
                category_id = dto.category_id,
                name = dto.name,
                description = dto.description,
                quantity = dto.quantity,
                unit_of_measure = dto.unit_of_measure,
                minimum_auction_price = dto.minimum_auction_price,
                current_market_price = dto.current_market_price,
                image_path = imagePath,
                image_name = imageName,
                status = InventoryItemStatus.Available,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            _context.InventoryItems.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetItem), new { id = item.Id }, new
            {
                message = "Item added successfully.",
                item_id = item.Id,
                item.name,
                item.minimum_auction_price,
                imageUrl = imagePath != null ? $"/api/inventory/image/{item.Id}" : null
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
        public async Task<IActionResult> UpdateItem(int id, [FromForm] UpdateInventoryItemDto dto)
        {
            var item = await _context.InventoryItems.FindAsync(id);
            if (item == null)
                return NotFound(new { message = "Item not found." });

            if (!CanAccessCompany(item.company_id))
                return Forbid();

            if (dto.image != null)
            {
                var validationResult = ValidateImage(dto.image);
                if (validationResult != null)
                    return BadRequest(new { message = validationResult });

                if (item.image_path != null && System.IO.File.Exists(item.image_path))
                    System.IO.File.Delete(item.image_path);

                var uploadResult = await SaveImage(dto.image, item.company_id);
                item.image_path = uploadResult.path;
                item.image_name = uploadResult.name;
            }

            item.name = dto.name ?? item.name;
            item.description = dto.description ?? item.description;
            item.quantity = dto.quantity ?? item.quantity;
            item.unit_of_measure = dto.unit_of_measure ?? item.unit_of_measure;
            item.minimum_auction_price = dto.minimum_auction_price ?? item.minimum_auction_price;
            item.current_market_price = dto.current_market_price ?? item.current_market_price;
            item.status = dto.status ?? item.status;
            item.updated_at = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Item updated successfully.", item_id = item.Id });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await _context.InventoryItems.FindAsync(id);
            if (item == null)
                return NotFound(new { message = "Item not found." });

            if (!CanAccessCompany(item.company_id))
                return Forbid();

            if (item.status == InventoryItemStatus.InAuction)
                return BadRequest(new { message = "Cannot delete an item in an active auction." });

            if (item.image_path != null && System.IO.File.Exists(item.image_path))
                System.IO.File.Delete(item.image_path);

            _context.InventoryItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Item deleted successfully." });
        }

        [HttpGet("image/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetImage(int id)
        {
            var item = await _context.InventoryItems.FindAsync(id);
            if (item == null || item.image_path == null)
                return NotFound(new { message = "Image not found." });

            if (!System.IO.File.Exists(item.image_path))
                return NotFound(new { message = "Image file not found." });

            var extension = Path.GetExtension(item.image_path).ToLower();
            var contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };

            var imageBytes = await System.IO.File.ReadAllBytesAsync(item.image_path);
            return File(imageBytes, contentType);
        }

        [HttpGet("company/{companyId}/stats")]
        public async Task<IActionResult> GetInventoryStats(int companyId)
        {
            if (!CanAccessCompany(companyId)) return Forbid();

            var items = await _context.InventoryItems
                .Where(i => i.company_id == companyId)
                .ToListAsync();

            return Ok(new
            {
                company_id = companyId,
                total_items = items.Count,
                available = items.Count(i => i.status == InventoryItemStatus.Available),
                in_auction = items.Count(i => i.status == InventoryItemStatus.InAuction),
                sold = items.Count(i => i.status == InventoryItemStatus.Sold),
                inactive = items.Count(i => i.status == InventoryItemStatus.Inactive),
                total_quantity = items.Sum(i => i.quantity),
                total_min_value = items.Sum(i => i.minimum_auction_price * i.quantity),
                avg_min_price = items.Any() ? Math.Round(items.Average(i => i.minimum_auction_price), 2) : 0
            });
        }

        // ── Private Helpers ───────────────────────────────────────────────────

        private string? ValidateImage(IFormFile file)
        {
            if (file.Length > MaxFileSize)
                return "Image size must be less than 5MB.";

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!_allowedExtensions.Contains(extension))
                return "Unsupported image type. Allowed types: JPG, PNG, WEBP.";

            return null;
        }

        private async Task<(string path, string name)> SaveImage(IFormFile file, int companyId)
        {
            var uploadPath = Path.Combine(
                _config["FileStorage:UploadPath"] ?? "uploads",
                "inventory",
                companyId.ToString());

            Directory.CreateDirectory(uploadPath);

            var fileName = $"{companyId}_{DateTime.UtcNow.Ticks}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return (filePath, file.FileName);
        }
    }

    public class CreateInventoryItemDto
    {
        public int company_id { get; set; }
        public int category_id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public decimal quantity { get; set; }
        public string unit_of_measure { get; set; }
        public decimal minimum_auction_price { get; set; }
        public decimal? current_market_price { get; set; }
        public IFormFile? image { get; set; }
    }

    public class UpdateInventoryItemDto
    {
        public string? name { get; set; }
        public string? description { get; set; }
        public decimal? quantity { get; set; }
        public string? unit_of_measure { get; set; }
        public decimal? minimum_auction_price { get; set; }
        public decimal? current_market_price { get; set; }
        public InventoryItemStatus? status { get; set; }
        public IFormFile? image { get; set; }
    }
}