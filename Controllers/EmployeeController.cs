using EmployeeAttendance.Auth;
using EmployeeAttendance.Models;
using EmployeeAttendance.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace EmployeeAttendance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeService _employeeService;

        public EmployeeController(EmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // GET api/employee
        [HttpGet]
        [RoleAuthorization("Manager", "Administrator")]
        public async Task<IActionResult> GetEmployees()
        {
            // Determine if user has access based on role
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (userRole == "Administrator")
            {
                // Admin can see all employees
                var allEmployees = await _employeeService.GetAllEmployeesAsync();
                return Ok(allEmployees);
            }
            else if (userRole == "Manager")
            {
                var isGeneralManager = bool.TryParse(
                    User.Claims.FirstOrDefault(c => c.Type == "IsGeneralManager")?.Value,
                    out bool isGenManager) && isGenManager;

                if (isGeneralManager)
                {
                    // General manager can see all employees
                    var allEmployees = await _employeeService.GetAllEmployeesAsync();
                    return Ok(allEmployees);
                }
                else
                {
                    // Department manager can only see their department
                    var departmentManaged = User.Claims.FirstOrDefault(c => c.Type == "DepartmentManaged")?.Value;
                    if (string.IsNullOrEmpty(departmentManaged))
                    {
                        return Forbid();
                    }

                    var departmentEmployees = await _employeeService.GetEmployeesByDepartmentAsync(departmentManaged);
                    return Ok(departmentEmployees);
                }
            }

            return Forbid();
        }

        // GET api/employee/{id}
        [HttpGet("{id}")]
        [RoleAuthorization("GeneralEmployee", "Manager", "Administrator")]
        public async Task<IActionResult> GetEmployee(int id)
        {
            // If general employee, check if it's their own record
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (userRole == "GeneralEmployee")
            {
                var userId = int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value, out int uId) ? uId : 0;
                if (id != userId)
                {
                    return Forbid();
                }
            }
            else if (userRole == "Manager" && User.Claims.FirstOrDefault(c => c.Type == "IsGeneralManager")?.Value != "True")
            {
                // Department manager - check if employee is in their department
                var departmentManaged = User.Claims.FirstOrDefault(c => c.Type == "DepartmentManaged")?.Value;
                var employee = await _employeeService.GetEmployeeByIdAsync(id);
                
                if (employee == null)
                {
                    return NotFound();
                }

                if (employee.Department != departmentManaged)
                {
                    return Forbid();
                }
            }

            var result = await _employeeService.GetEmployeeByIdAsync(id);
            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        // POST api/employee
        [HttpPost]
        [RoleAuthorization("Administrator")]
        public async Task<IActionResult> CreateEmployee([FromBody] Employee employee)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var id = await _employeeService.CreateEmployeeAsync(employee);
            return CreatedAtAction(nameof(GetEmployee), new { id }, employee);
        }

        // PUT api/employee/{id}
        [HttpPut("{id}")]
        [RoleAuthorization("Administrator")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] Employee employee)
        {
            if (id != employee.EmployeeID)
            {
                return BadRequest("ID mismatch");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _employeeService.UpdateEmployeeAsync(employee);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE api/employee/{id}
        [HttpDelete("{id}")]
        [RoleAuthorization("Administrator")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var result = await _employeeService.DeleteEmployeeAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // POST api/employee/import
        [HttpPost("import")]
        [RoleAuthorization("Administrator")]
        public async Task<IActionResult> ImportEmployees([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is empty");
            }

            var employees = new List<Employee>();
            try
            {
                using var reader = new StreamReader(file.OpenReadStream());
                var fileContent = await reader.ReadToEndAsync();

                if (file.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    employees = JsonSerializer.Deserialize<List<Employee>>(fileContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<Employee>();
                }
                else if (file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    // Simple CSV parsing (could use a library for more robust parsing)
                    var lines = fileContent.Split('\n');
                    if (lines.Length <= 1) return BadRequest("CSV file has no data");

                    var headers = lines[0].Split(',');
                    for (int i = 1; i < lines.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[i])) continue;
                        
                        var values = lines[i].Split(',');
                        var employee = new Employee
                        {
                            FirstName = values[0],
                            LastName = values[1],
                            Gender = values[2],
                            Department = values[3],
                            PhoneNumber = values[4],
                            IsIntern = bool.Parse(values[5]),
                            Role = values[6],
                            Band = values[7],
                            TechnicalDirection = values[8],
                            HasCodingSkill = bool.Parse(values[9])
                        };
                        employees.Add(employee);
                    }
                }
                else
                {
                    return BadRequest("Unsupported file format. Use .json or .csv");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error processing file: {ex.Message}");
            }

            if (employees.Count == 0)
            {
                return BadRequest("No valid employee records found in file");
            }

            var importedCount = await _employeeService.ImportEmployeesFromCsvAsync(employees);
            return Ok(new { message = $"Successfully imported {importedCount} employees" });
        }
    }
} 