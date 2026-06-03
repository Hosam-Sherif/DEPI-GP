using System.Collections.Generic;

public class DashboardStatisticsDto
{
    public decimal TotalRevenue { get; set; }

    public int TotalOrders { get; set; }

    public int ActiveAuctions { get; set; }

    public int InventoryCount { get; set; }

    public List<TopProductDto> TopProducts { get; set; }
}