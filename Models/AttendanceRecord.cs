namespace EmployeeAttendance.Models
{
    public class AttendanceRecord
    {
        public int RecordID { get; set; }
        public int EmployeeID { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
    }
} 