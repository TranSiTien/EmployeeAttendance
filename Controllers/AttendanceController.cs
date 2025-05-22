using EmployeeAttendance.Auth;
using EmployeeAttendance.Models;
using EmployeeAttendance.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EmployeeAttendance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly AttendanceService _attendanceService;

        public AttendanceController(AttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        // GET api/attendance
        [HttpGet]
        [RoleAuthorization("Administrator")]
        public async Task<IActionResult> GetAllAttendance([FromQuery] DateTime? date = null)
        {
            var records = await _attendanceService.GetAllAttendanceRecordsAsync(date);
            return Ok(records);
        }

        // GET api/attendance/department/{department}
        [HttpGet("department/{department}")]
        [RoleAuthorization("Manager", "Administrator")]
        public async Task<IActionResult> GetDepartmentAttendance(string department, [FromQuery] DateTime? date = null)
        {
            // Check if user has permission to access this department's data
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (userRole == "Manager" && User.Claims.FirstOrDefault(c => c.Type == "IsGeneralManager")?.Value != "True")
            {
                var departmentManaged = User.Claims.FirstOrDefault(c => c.Type == "DepartmentManaged")?.Value;
                if (departmentManaged != department)
                {
                    return Forbid();
                }
            }

            var records = await _attendanceService.GetAttendanceRecordsByDepartmentAsync(department, date);
            return Ok(records);
        }

        // GET api/attendance/{employeeId}
        [HttpGet("{employeeId}")]
        [RoleAuthorization("GeneralEmployee", "Manager", "Administrator")]
        public async Task<IActionResult> GetEmployeeAttendance(int employeeId)
        {
            // Check if user has permission to access this employee's data
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (userRole == "GeneralEmployee")
            {
                var userId = int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value, out int uId) ? uId : 0;
                if (employeeId != userId)
                {
                    return Forbid();
                }
            }

            var records = await _attendanceService.GetAttendanceRecordsByEmployeeIdAsync(employeeId);
            return Ok(records);
        }

        // GET api/attendance/{employeeId}/today
        [HttpGet("{employeeId}/today")]
        [RoleAuthorization("GeneralEmployee", "Manager", "Administrator")]
        public async Task<IActionResult> GetEmployeeTodayAttendance(int employeeId)
        {
            // Check if user has permission to access this employee's data
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (userRole == "GeneralEmployee")
            {
                var userId = int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value, out int uId) ? uId : 0;
                if (employeeId != userId)
                {
                    return Forbid();
                }
            }

            var record = await _attendanceService.GetTodayAttendanceForEmployeeAsync(employeeId);
            if (record == null)
            {
                return NotFound();
            }

            return Ok(record);
        }

        // POST api/attendance/checkin
        [HttpPost("checkin")]
        [RoleAuthorization("GeneralEmployee")]
        public async Task<IActionResult> CheckIn([FromBody] CheckInOutModel model)
        {
            // Verify it's the employee's own record
            var userId = int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value, out int uId) ? uId : 0;
            if (model.EmployeeId != userId)
            {
                return Forbid();
            }

            try
            {
                var recordId = await _attendanceService.CheckInAsync(model.EmployeeId);
                return Ok(new { recordId, message = "Check-in successful" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST api/attendance/checkout
        [HttpPost("checkout")]
        [RoleAuthorization("GeneralEmployee")]
        public async Task<IActionResult> CheckOut([FromBody] CheckInOutModel model)
        {
            // Verify it's the employee's own record
            var userId = int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value, out int uId) ? uId : 0;
            if (model.EmployeeId != userId)
            {
                return Forbid();
            }

            var result = await _attendanceService.CheckOutAsync(model.EmployeeId);
            if (!result)
            {
                return BadRequest("No active check-in found for today or already checked out");
            }

            return Ok(new { message = "Check-out successful" });
        }
    }

    public class CheckInOutModel
    {
        public int EmployeeId { get; set; }
    }
} 