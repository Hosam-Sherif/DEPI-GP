using Mazaad.Domain.Models;
using Mazaad.Domain.Enums;
using Mazaad.Infrastructure.Persistence;
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
    public class InventoryController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxFileSize = 5 * 1024 * 1024;

        public InventoryController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpGet("company/{companyId}")]
        public async Task<IActionResult> GetCompanyInventory(int companyId, [FromQuery] InventoryItemStatus? status = null)
        {
            var query = _context.InventoryItems
                .Include(i => i.Category)
                .Where(i => i.company_id == companyId);

            if (status.HasValue)
                query = query.Where(i => i.status == status.Value);

            var items = await query
                .OrderByDescending(i => i.created_at)
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
                    ImageUrl = i.image_path != null ? $"/api/inventory/image/{i.Id}" : null,
                    CategoryName = i.Category.CategoryName,
                    i.created_at,
                    i.updated_at
                })
                .ToListAsync();

            return Ok(new
            {
                company_id = companyId,
                total_items = items.Count,
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
                ImageUrl = item.image_path != null ? $"/api/inventory/image/{item.Id}" : null,
                CategoryName = item.Category.CategoryName,
                CompanyName = item.Company.CompanyName,
                item.created_at,
                item.updated_at
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateItem([FromForm] CreateInventoryItemDto dto)
        {
            var company = await _context.Companies.FindAsync(dto.company_id);
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

                var uploadResult = await SaveImage(dto.image, dto.company_id);
                imagePath = uploadResult.path;
                imageName = uploadResult.name;
            }

            var item = new InventoryItem
            {
                company_id = dto.company_id,
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
                ImageUrl = imagePath != null ? $"/api/inventory/image/{item.Id}" : null
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromForm] UpdateInventoryItemDto dto)
        {
            var item = await _context.InventoryItems.FindAsync(id);
            if (item == null)
                return NotFound(new { message = "Item not found." });

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
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await _context.InventoryItems.FindAsync(id);
            if (item == null)
                return NotFound(new { message = "Item not found." });

            if (item.status == InventoryItemStatus.InAuction)
                return BadRequest(new { message = "Cannot delete an item in an active auction." });

            if (item.image_path != null && System.IO.File.Exists(item.image_path))
                System.IO.File.Delete(item.image_path);

            _context.InventoryItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Item deleted successfully." });
        }

        [HttpGet("image/{id}")]
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
            var uploadPath = Path.Combine(_config["FileStorage:UploadPath"] ?? "uploads", "inventory", companyId.ToString());
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
