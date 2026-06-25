using System.Threading.Tasks;
using Mazaad.Application.DTOs.Sales;

namespace Mazaad.Application.Interfaces.Services
{
    public interface ISalesOperationsService
    {
        Task<DashboardStatisticsDto> GetDashboardAsync(int companyId);
    }
}
