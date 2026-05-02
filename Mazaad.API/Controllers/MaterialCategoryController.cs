using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mazaad.Application.DTOs;
using Mazaad.Application.Interfaces;

namespace Mazaad.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaterialCategoryController : ControllerBase
    {
        private readonly IMaterialCategoryService _categoryService;

        public MaterialCategoryController(IMaterialCategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null) return NotFound();
            return Ok(category);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto request)
        {
            var createdCategory = await _categoryService.CreateCategoryAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = createdCategory.Id }, createdCategory);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _categoryService.DeleteCategoryAsync(id);
            if (!success) return BadRequest("Could not delete category.");
            return NoContent();
        }
    }
}