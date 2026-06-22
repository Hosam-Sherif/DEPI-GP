using Mazaad.Application.DTOs.User;
using Mazaad.Application.Interfaces;
using Mazaad.Domain.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
namespace Mazaad.Infrastructure.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public EmployeeService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IEnumerable<EmployeeDto>> GetCompanyEmployeesAsync(int adminCompanyId)
        {
            return await _userManager.Users
                .Where(u => u.CompanyId == adminCompanyId)
                .Select(u => new EmployeeDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email ?? string.Empty,
                    JobTitle = u.JobTitle,
                    IsActive = u.IsActive,
                    CompanyId = u.CompanyId
                })
                .ToListAsync();
        }

        public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id, int adminCompanyId)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.CompanyId == adminCompanyId);

            if (user == null) return null;

            return new EmployeeDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                JobTitle = user.JobTitle,
                IsActive = user.IsActive,
                CompanyId = user.CompanyId
            };
        }

        public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto, int adminCompanyId)
        {
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                JobTitle = dto.JobTitle,
                CompanyId = adminCompanyId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                var firstError = result.Errors.FirstOrDefault()?.Description ?? "Error ";
                throw new Exception(firstError);
            }

            await _userManager.AddToRoleAsync(user, "CompanyUser");

            return new EmployeeDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                JobTitle = user.JobTitle,
                IsActive = user.IsActive,
                CompanyId = user.CompanyId
            };
        }

        public async Task<EmployeeDto> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto, int adminCompanyId)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.CompanyId == adminCompanyId);

            if (user == null)
            {
                throw new Exception("Not Found.");
            }

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.UserName = dto.Email;
            user.JobTitle = dto.JobTitle;
            user.IsActive = dto.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var firstError = result.Errors.FirstOrDefault()?.Description ?? "Error !";
                throw new Exception(firstError);
            }

            return new EmployeeDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                JobTitle = user.JobTitle,
                IsActive = user.IsActive,
                CompanyId = user.CompanyId
            };
        }
        public async Task<bool> DeleteEmployeeAsync(int id, int adminCompanyId)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.CompanyId == adminCompanyId);

            if (user == null)
            {
                throw new Exception("Employee not found.");
            }

            user.IsActive = false; // Soft Delete
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
    }
}
