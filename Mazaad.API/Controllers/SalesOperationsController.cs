using Mazaad.API.Filters;
using Mazaad.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Mazaad.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [CompanyOwnership]
    public class SalesOperationsController : ControllerBase
    {
        private readonly ISalesOperationsService _salesOperations;

        public SalesOperationsController(ISalesOperationsService salesOperations)
        {
            _salesOperations = salesOperations;
        }

        /// <summary>
        /// إحصائيات Dashboard مختصرة للشركة (إجمالي الإيرادات، عدد الطلبات،
        /// المزادات النشطة، عدد المخزون). Scoped لشركة اليوزر المسجل دخوله فقط.
        /// </summary>
        [HttpGet("company/{companyId}/dashboard")]
        public async Task<IActionResult> GetDashboard(int companyId)
        {
            var result = await _salesOperations.GetDashboardAsync(companyId);
            return Ok(result);
        }
    }
}