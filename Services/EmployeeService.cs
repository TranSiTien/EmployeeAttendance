using System.Data;
using EmployeeAttendance.Data;
using EmployeeAttendance.Models;

namespace EmployeeAttendance.Services
{
    public class EmployeeService
    {
        private readonly DbContext _dbContext;

        public EmployeeService(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Employee>> GetAllEmployeesAsync()
        {
            var query = "SELECT * FROM Employees";
            var result = await _dbContext.ExecuteQueryAsync(query);

            return MapEmployeesFromDataTable(result);
        }

        public async Task<List<Employee>> GetEmployeesByDepartmentAsync(string department)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@Department", department }
            };

            var query = "SELECT * FROM Employees WHERE Department = @Department";
            var result = await _dbContext.ExecuteQueryAsync(query, parameters);

            return MapEmployeesFromDataTable(result);
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@EmployeeID", id }
            };

            var query = "SELECT * FROM Employees WHERE EmployeeID = @EmployeeID";
            var result = await _dbContext.ExecuteQueryAsync(query, parameters);

            if (result.Rows.Count == 0)
                return null;

            return MapEmployeeFromDataRow(result.Rows[0]);
        }

        public async Task<int> CreateEmployeeAsync(Employee employee)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@LastName", employee.LastName },
                { "@FirstName", employee.FirstName },
                { "@Gender", employee.Gender },
                { "@Department", employee.Department },
                { "@PhoneNumber", employee.PhoneNumber },
                { "@IsIntern", employee.IsIntern },
                { "@Role", employee.Role },
                { "@Band", employee.Band },
                { "@TechnicalDirection", employee.TechnicalDirection },
                { "@HasCodingSkill", employee.HasCodingSkill }
            };

            var query = @"
                INSERT INTO Employees (LastName, FirstName, Gender, Department, PhoneNumber, IsIntern, Role, Band, TechnicalDirection, HasCodingSkill)
                VALUES (@LastName, @FirstName, @Gender, @Department, @PhoneNumber, @IsIntern, @Role, @Band, @TechnicalDirection, @HasCodingSkill);
                SELECT SCOPE_IDENTITY();";

            var result = await _dbContext.ExecuteScalarAsync(query, parameters);
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateEmployeeAsync(Employee employee)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@EmployeeID", employee.EmployeeID },
                { "@LastName", employee.LastName },
                { "@FirstName", employee.FirstName },
                { "@Gender", employee.Gender },
                { "@Department", employee.Department },
                { "@PhoneNumber", employee.PhoneNumber },
                { "@IsIntern", employee.IsIntern },
                { "@Role", employee.Role },
                { "@Band", employee.Band },
                { "@TechnicalDirection", employee.TechnicalDirection },
                { "@HasCodingSkill", employee.HasCodingSkill }
            };

            var query = @"
                UPDATE Employees
                SET LastName = @LastName, 
                    FirstName = @FirstName,
                    Gender = @Gender,
                    Department = @Department,
                    PhoneNumber = @PhoneNumber,
                    IsIntern = @IsIntern,
                    Role = @Role,
                    Band = @Band,
                    TechnicalDirection = @TechnicalDirection,
                    HasCodingSkill = @HasCodingSkill
                WHERE EmployeeID = @EmployeeID";

            var rowsAffected = await _dbContext.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@EmployeeID", id }
            };

            var query = "DELETE FROM Employees WHERE EmployeeID = @EmployeeID";
            var rowsAffected = await _dbContext.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<int> ImportEmployeesFromCsvAsync(List<Employee> employees)
        {
            int successCount = 0;
            foreach (var employee in employees)
            {
                try
                {
                    await CreateEmployeeAsync(employee);
                    successCount++;
                }
                catch (Exception)
                {
                    // Log error or handle exception
                }
            }
            return successCount;
        }

        private List<Employee> MapEmployeesFromDataTable(DataTable dataTable)
        {
            var employees = new List<Employee>();
            foreach (DataRow row in dataTable.Rows)
            {
                employees.Add(MapEmployeeFromDataRow(row));
            }
            return employees;
        }

        private Employee MapEmployeeFromDataRow(DataRow row)
        {
            return new Employee
            {
                EmployeeID = Convert.ToInt32(row["EmployeeID"]),
                LastName = row["LastName"].ToString()!,
                FirstName = row["FirstName"].ToString()!,
                Gender = row["Gender"].ToString()!,
                Department = row["Department"].ToString()!,
                PhoneNumber = row["PhoneNumber"].ToString()!,
                IsIntern = Convert.ToBoolean(row["IsIntern"]),
                Role = row["Role"].ToString()!,
                Band = row["Band"].ToString()!,
                TechnicalDirection = row["TechnicalDirection"]?.ToString() ?? string.Empty,
                HasCodingSkill = Convert.ToBoolean(row["HasCodingSkill"])
            };
        }
    }
} 