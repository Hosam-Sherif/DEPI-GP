//using Mazaad.Application.DTOs.User;
//using Mazaad.Application.Interfaces;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;

//namespace Mazaad.API.Controllers
//{

//    [Authorize(Roles = "CompanyAdmin")]
//    [Route("api/[controller]")]
//    [ApiController]
//    public class EmployeesController : ControllerBase
//    {
//        private readonly IEmployeeService _employeeService;

//        public EmployeesController(IEmployeeService employeeService)
//        {
//            _employeeService = employeeService;
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetEmployees()
//        {
//            var adminCompanyId = int.Parse(User.FindFirst("CompanyId")?.Value ?? "0");
//            if (adminCompanyId == 0) return BadRequest(new { message = "Invalid company identifier." });

//            var employees = await _employeeService.GetCompanyEmployeesAsync(adminCompanyId);
//            return Ok(employees);
//        }

//        [HttpGet("{id}")]
//        public async Task<IActionResult> GetEmployeeById(int id)
//        {
//            var adminCompanyId = int.Parse(User.FindFirst("CompanyId")?.Value ?? "0");
//            if (adminCompanyId == 0) return BadRequest(new { message = "Invalid company identifier." });

//            var employee = await _employeeService.GetEmployeeByIdAsync(id, adminCompanyId);
//            if (employee == null) return NotFound(new { message = "Employee not found." });

//            return Ok(employee);
//        }

//        [HttpPost]
//        public async Task<IActionResult> CreateEmployee(CreateEmployeeDto createEmployeeDto)
//        {
//            var adminCompanyId = int.Parse(User.FindFirst("CompanyId")?.Value ?? "0");
//            if (adminCompanyId == 0) return BadRequest(new { message = "Invalid company identifier." });

//            try
//            {
//                var result = await _employeeService.CreateEmployeeAsync(createEmployeeDto, adminCompanyId);
//                return Ok(result);
//            }
//            catch (Exception ex)
//            {
//                return BadRequest(new { message = ex.Message });
//            }
//        }

//        [Authorize(Roles = "CompanyAdmin")]
//        [HttpPut("{id}")]
//        public async Task<IActionResult> UpdateEmployee(int id, UpdateEmployeeDto updateEmployeeDto)
//        {
//            var adminCompanyId = int.Parse(User.FindFirst("CompanyId")?.Value ?? "0");
//            if (adminCompanyId == 0) return BadRequest(new { message = "Invalid company identifier." });

//            try
//            {
//                var result = await _employeeService.UpdateEmployeeAsync(id, updateEmployeeDto, adminCompanyId);
//                return Ok(result);
//            }
//            catch (Exception ex)
//            {
//                return BadRequest(new { message = ex.Message });
//            }
//        }

//        [HttpDelete("{id}")]
//        public async Task<IActionResult> DeleteEmployee(int id)
//        {
//            var adminCompanyId = int.Parse(User.FindFirst("CompanyId")?.Value ?? "0");
//            if (adminCompanyId == 0) return BadRequest(new { message = "Invalid company identifier." });

//            try
//            {
//                await _employeeService.DeleteEmployeeAsync(id, adminCompanyId);
//                return Ok(new { message = "Employee account deactivated successfully." });
//            }
//            catch (Exception ex)
//            {
//                return BadRequest(new { message = ex.Message });
//            }
//        }
//    }
//}

