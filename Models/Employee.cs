namespace EmployeeAttendance.Models
{
    public class Employee
    {
        public int EmployeeID { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsIntern { get; set; }
        public string Role { get; set; } = string.Empty; // Developer, QA, Manager
        public string Band { get; set; } = string.Empty;
        public string TechnicalDirection { get; set; } = string.Empty;
        public bool HasCodingSkill { get; set; }
    }
} 