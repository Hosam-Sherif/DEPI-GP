using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mazaad.Domain.Enums;
using Mazaad.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Mazaad.Infrastructure.Services
{
    /// <summary>
    /// Background service that runs every 60 seconds and automatically transitions:
    ///   Upcoming → Active  (when StartDate is reached)
    ///   Active   → Ended   (when EndDate has passed)
    /// </summary>
    public class AuctionStatusUpdaterService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AuctionStatusUpdaterService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(60);

        public AuctionStatusUpdaterService(
            IServiceScopeFactory scopeFactory,
            ILogger<AuctionStatusUpdaterService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AuctionStatusUpdater started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateStatusesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in AuctionStatusUpdater.");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task UpdateStatusesAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var now = DateTime.UtcNow;

            // Upcoming → Active
            var toActivate = await db.Listings
                .Where(l => !l.IsDeleted
                         && l.Status == ListingStatus.Upcoming
                         && l.StartDate <= now)
                .ToListAsync();

            foreach (var l in toActivate)
            {
                l.Status = ListingStatus.Active;
                l.UpdatedAt = now;
            }

            // Active → Ended
            var toEnd = await db.Listings
                .Where(l => !l.IsDeleted
                         && l.Status == ListingStatus.Active
                         && l.EndDate <= now)
                .ToListAsync();

            foreach (var l in toEnd)
            {
                l.Status = ListingStatus.Ended;
                l.UpdatedAt = now;
            }

            if (toActivate.Count > 0 || toEnd.Count > 0)
            {
                await db.SaveChangesAsync();
                _logger.LogInformation(
                    "AuctionStatusUpdater: {Activated} activated, {Ended} ended.",
                    toActivate.Count, toEnd.Count);
            }
        }
    }
}