using Mazaad.Application.DTOs.Store;
using Mazaad.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mazaad.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StoreController : ControllerBase
    {
        private readonly IStoreService _storeService;

        public StoreController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        // POST /api/store/create
        [HttpPost("create")]
        [Authorize(Roles = "CompanyAdmin")]
        public async Task<IActionResult> CreateStore([FromForm] CreateStoreDto dto, IFormFile? logo)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var result = await _storeService.CreateStoreAsync(companyId.Value, dto, logo);
            return result.Succeeded ? Ok(result.Data) : BadRequest(result.Error);
        }

        // GET /api/store/my-store
        [HttpGet("my-store")]
        [Authorize(Roles = "CompanyAdmin,CompanyUser")]
        public async Task<IActionResult> GetMyStore()
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var result = await _storeService.GetMyStoreAsync(companyId.Value);
            return result.Succeeded ? Ok(result.Data) : NotFound(result.Error);
        }

        // GET /api/store/{slug} — Public
        [HttpGet("{slug}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStoreBySlug(string slug)
        {
            var result = await _storeService.GetStoreBySlugAsync(slug);
            return result.Succeeded ? Ok(result.Data) : NotFound(result.Error);
        }

        // PUT /api/store/update
        [HttpPut("update")]
        [Authorize(Roles = "CompanyAdmin")]
        public async Task<IActionResult> UpdateStore([FromForm] UpdateStoreDto dto, IFormFile? logo)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var result = await _storeService.UpdateStoreAsync(companyId.Value, dto, logo);
            return result.Succeeded ? Ok(result.Data) : BadRequest(result.Error);
        }

        // GET /api/store/check-slug/{slug} — Public
        [HttpGet("check-slug/{slug}")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckSlug(string slug)
        {
            var result = await _storeService.IsSlugAvailableAsync(slug);
            return Ok(new { slug, isAvailable = result.Data });
        }

        private int? GetCompanyId()
        {
            var claim = User.FindFirst("CompanyId")?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}