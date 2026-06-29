// Mazaad.API/Controllers/AnalyticsController.cs

using Mazaad.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mazaad.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analytics;

        public AnalyticsController(IAnalyticsService analytics)
        {
            _analytics = analytics;
        }

        /// <summary>
        /// Average, highest, and lowest bid price per material category (active listings only).
        /// </summary>
        [HttpGet("asset-value-index")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAssetValueIndex()
        {
            var data = await _analytics.GetAssetValueIndexAsync();
            return Ok(data);
        }

        /// <summary>
        /// Bid and order activity grouped by city/region with a normalized demand score (0–100).
        /// </summary>
        [HttpGet("regional-demand-heatmap")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRegionalDemandHeatmap()
        {
            var data = await _analytics.GetRegionalDemandHeatmapAsync();
            return Ok(data);
        }

        /// <summary>
        /// Most recent completed orders used as market price benchmarks.
        /// </summary>
        [HttpGet("recent-benchmarks")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetRecentBenchmarks([FromQuery] int count = 10)
        {
            if (count < 1 || count > 50)
                return BadRequest("count must be between 1 and 50.");

            var data = await _analytics.GetRecentBenchmarksAsync(count);
            return Ok(data);
        }

        /// <summary>
        /// Listings with the highest bidding momentum in the last 7 days vs. the previous 7 days.
        /// </summary>
        [HttpGet("momentum-movers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetMomentumMovers([FromQuery] int top = 10)
        {
            if (top < 1 || top > 50)
                return BadRequest("top must be between 1 and 50.");

            var data = await _analytics.GetMomentumMoversAsync(top);
            return Ok(data);
        }
    }
}