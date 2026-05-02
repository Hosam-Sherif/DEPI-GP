using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mazaad.Application.DTOs;
using Mazaad.Application.Interfaces;
using Mazaad.Domain.Models;
using Mazaad.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mazaad.Application.Services
{
    public class MaterialCategoryService : IMaterialCategoryService
    {
        private readonly AppDbContext _context;

        public MaterialCategoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync()
        {
            var categories = await _context.MaterialCategories.ToListAsync();
            return categories.Select(c => new CategoryResponseDto
            {
                Id = c.Id,
                CategoryName = c.CategoryName,
                Description = c.Description,
                UnitOfMeasure = c.UnitOfMeasure
            });
        }

        public async Task<CategoryResponseDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _context.MaterialCategories.FindAsync(id);
            if (category == null) return null;

            return new CategoryResponseDto
            {
                Id = category.Id,
                CategoryName = category.CategoryName,
                Description = category.Description,
                UnitOfMeasure = category.UnitOfMeasure
            };
        }

        public async Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryDto request)
        {
            var category = new Material_Categories
            {
                CategoryName = request.CategoryName,
                Description = request.Description,
                UnitOfMeasure = request.UnitOfMeasure,
                CreatedAt = DateTime.UtcNow
            };

            _context.MaterialCategories.Add(category);
            await _context.SaveChangesAsync();

            return new CategoryResponseDto
            {
                Id = category.Id,
                CategoryName = category.CategoryName,
                Description = category.Description,
                UnitOfMeasure = category.UnitOfMeasure
            };
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.MaterialCategories.FindAsync(id);
            if (category == null) return false;

            _context.MaterialCategories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
