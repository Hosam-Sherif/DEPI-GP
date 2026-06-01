using Mazaad.Application.DTOs.Inventory;
using Mazaad.Application.Interfaces.Repositories;
using Mazaad.Application.Interfaces.Services;
using Mazaad.Domain.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Mazaad.Infrastructure.Services.Inventory
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _repository;

        private readonly IWebHostEnvironment _environment;

        public InventoryService(
            IInventoryRepository repository,
            IWebHostEnvironment environment)
        {
            _repository = repository;
            _environment = environment;
        }

        public async Task<object> CreateAsync(
            int companyId,
            CreateInventoryDto dto)
        {
            string imageName =
                Guid.NewGuid() +
                Path.GetExtension(dto.Image.FileName);

            string uploadsFolder =
                Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "products");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(
                    uploadsFolder);
            }

            string imagePath =
                Path.Combine(
                    uploadsFolder,
                    imageName);

            using var stream =
                new FileStream(
                    imagePath,
                    FileMode.Create);

            await dto.Image.CopyToAsync(stream);

            var listing = new Listings
            {
                title = dto.ProductName,

                description = dto.Description,

                starting_price =
                    dto.StartingPrice,

                current_price =
                    dto.StartingPrice,

                quantity = dto.Quantity,

                company_id = companyId,

                image_url =
                    $"/uploads/products/{imageName}",

                created_at = DateTime.UtcNow
            };

            await _repository.AddAsync(listing);

            await _repository.SaveChangesAsync();

            return new
            {
                listing.ID,
                listing.title,
                listing.image_url
            };
        }

        public async Task<object> UpdateAsync(
            int id,
            UpdateInventoryDto dto)
        {
            var listing =
                await _repository.GetByIdAsync(id);

            if (listing == null)
                throw new Exception("Listing not found");

            listing.title = dto.ProductName;

            listing.description = dto.Description;

            listing.starting_price =
                dto.StartingPrice;

            listing.quantity = dto.Quantity;

            if (dto.Image != null)
            {
                string imageName =
                    Guid.NewGuid() +
                    Path.GetExtension(dto.Image.FileName);

                string uploadsFolder =
                    Path.Combine(
                        _environment.WebRootPath,
                        "uploads",
                        "products");

                string imagePath =
                    Path.Combine(
                        uploadsFolder,
                        imageName);

                using var stream =
                    new FileStream(
                        imagePath,
                        FileMode.Create);

                await dto.Image.CopyToAsync(stream);

                listing.image_url =
                    $"/uploads/products/{imageName}";
            }

            await _repository.SaveChangesAsync();

            return listing;
        }

        public async Task DeleteAsync(int id)
        {
            var listing =
                await _repository.GetByIdAsync(id);

            if (listing == null)
                throw new Exception("Listing not found");

            _repository.Delete(listing);

            await _repository.SaveChangesAsync();
        }

        public async Task<object>
            GetCompanyInventoryAsync(int companyId)
        {
            var listings =
                await _repository
                    .GetCompanyInventoryAsync(companyId);

            return listings.Select(x => new
            {
                x.ID,
                x.title,
                x.current_price,
                x.quantity,
                x.image_url,
                x.created_at
            });
        }
    }
}