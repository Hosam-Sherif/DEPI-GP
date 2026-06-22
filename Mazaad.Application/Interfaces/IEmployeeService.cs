using Mazaad.Application.DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mazaad.Application.Interfaces
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeDto>> GetCompanyEmployeesAsync(int adminCompanyId);
        Task<EmployeeDto?> GetEmployeeByIdAsync(int id, int adminCompanyId);
        Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto, int adminCompanyId);
        Task<EmployeeDto> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto, int adminCompanyId);
        Task<bool> DeleteEmployeeAsync(int id, int adminCompanyId);
    }
}
