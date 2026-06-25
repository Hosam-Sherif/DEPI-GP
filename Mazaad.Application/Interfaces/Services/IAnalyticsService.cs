using Mazaad.Application.DTOs.Analytics;

namespace Mazaad.Application.Interfaces.Services
{
    public interface IAnalyticsService
    {
        /// <summary>Average, highest, and lowest bid price per material category.</summary>
        Task<IEnumerable<AssetValueIndexDto>> GetAssetValueIndexAsync();

        /// <summary>Bid and order demand grouped by city/region with normalized score.</summary>
        Task<IEnumerable<RegionalDemandDto>> GetRegionalDemandHeatmapAsync();

        /// <summary>Last N completed orders used as market price benchmarks.</summary>
        Task<IEnumerable<RecentBenchmarkDto>> GetRecentBenchmarksAsync(int count = 10);

        /// <summary>Top listings by bid activity in the last 7 days.</summary>
        Task<IEnumerable<MomentumMoverDto>> GetMomentumMoversAsync(int top = 10);
    }
}