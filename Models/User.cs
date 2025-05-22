namespace EmployeeAttendance.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // GeneralEmployee, Manager, Administrator
        public string? DepartmentManaged { get; set; } // For DepartmentManager role
        public bool IsGeneralManager { get; set; } // For Manager role
    }
} 