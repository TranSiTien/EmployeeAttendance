using System.Data;
using EmployeeAttendance.Data;
using EmployeeAttendance.Models;

namespace EmployeeAttendance.Services
{
    public class AttendanceService
    {
        private readonly DbContext _dbContext;

        public AttendanceService(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<AttendanceRecord>> GetAttendanceRecordsByEmployeeIdAsync(int employeeId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@EmployeeID", employeeId }
            };

            var query = "SELECT * FROM AttendanceRecords WHERE EmployeeID = @EmployeeID ORDER BY Date DESC";
            var result = await _dbContext.ExecuteQueryAsync(query, parameters);

            return MapAttendanceRecordsFromDataTable(result);
        }

        public async Task<List<AttendanceRecord>> GetAttendanceRecordsByDepartmentAsync(string department, DateTime? date = null)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@Department", department }
            };

            var query = @"
                SELECT ar.* 
                FROM AttendanceRecords ar
                JOIN Employees e ON ar.EmployeeID = e.EmployeeID
                WHERE e.Department = @Department";

            if (date.HasValue)
            {
                query += " AND ar.Date = @Date";
                parameters.Add("@Date", date.Value.Date);
            }

            query += " ORDER BY ar.Date DESC, e.LastName, e.FirstName";

            var result = await _dbContext.ExecuteQueryAsync(query, parameters);

            return MapAttendanceRecordsFromDataTable(result);
        }

        public async Task<List<AttendanceRecord>> GetAllAttendanceRecordsAsync(DateTime? date = null)
        {
            var parameters = new Dictionary<string, object>();
            var query = "SELECT * FROM AttendanceRecords";

            if (date.HasValue)
            {
                query += " WHERE Date = @Date";
                parameters.Add("@Date", date.Value.Date);
            }

            query += " ORDER BY Date DESC, EmployeeID";

            var result = await _dbContext.ExecuteQueryAsync(query, parameters);

            return MapAttendanceRecordsFromDataTable(result);
        }

        public async Task<AttendanceRecord?> GetAttendanceRecordByIdAsync(int recordId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@RecordID", recordId }
            };

            var query = "SELECT * FROM AttendanceRecords WHERE RecordID = @RecordID";
            var result = await _dbContext.ExecuteQueryAsync(query, parameters);

            if (result.Rows.Count == 0)
                return null;

            return MapAttendanceRecordFromDataRow(result.Rows[0]);
        }

        public async Task<AttendanceRecord?> GetTodayAttendanceForEmployeeAsync(int employeeId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@EmployeeID", employeeId },
                { "@Today", DateTime.Today }
            };

            var query = "SELECT * FROM AttendanceRecords WHERE EmployeeID = @EmployeeID AND Date = @Today";
            var result = await _dbContext.ExecuteQueryAsync(query, parameters);

            if (result.Rows.Count == 0)
                return null;

            return MapAttendanceRecordFromDataRow(result.Rows[0]);
        }

        public async Task<int> CheckInAsync(int employeeId)
        {
            // Check if already checked in today
            var existingRecord = await GetTodayAttendanceForEmployeeAsync(employeeId);
            if (existingRecord != null)
            {
                throw new InvalidOperationException("Employee already checked in today");
            }

            var parameters = new Dictionary<string, object>
            {
                { "@EmployeeID", employeeId },
                { "@Date", DateTime.Today },
                { "@CheckInTime", DateTime.Now.TimeOfDay }
            };

            var query = @"
                INSERT INTO AttendanceRecords (EmployeeID, Date, CheckInTime)
                VALUES (@EmployeeID, @Date, @CheckInTime);
                SELECT SCOPE_IDENTITY();";

            var result = await _dbContext.ExecuteScalarAsync(query, parameters);
            return Convert.ToInt32(result);
        }

        public async Task<bool> CheckOutAsync(int employeeId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@EmployeeID", employeeId },
                { "@Date", DateTime.Today },
                { "@CheckOutTime", DateTime.Now.TimeOfDay }
            };

            var query = @"
                UPDATE AttendanceRecords 
                SET CheckOutTime = @CheckOutTime
                WHERE EmployeeID = @EmployeeID AND Date = @Date AND CheckOutTime IS NULL";

            var rowsAffected = await _dbContext.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        private List<AttendanceRecord> MapAttendanceRecordsFromDataTable(DataTable dataTable)
        {
            var records = new List<AttendanceRecord>();
            foreach (DataRow row in dataTable.Rows)
            {
                records.Add(MapAttendanceRecordFromDataRow(row));
            }
            return records;
        }

        private AttendanceRecord MapAttendanceRecordFromDataRow(DataRow row)
        {
            return new AttendanceRecord
            {
                RecordID = Convert.ToInt32(row["RecordID"]),
                EmployeeID = Convert.ToInt32(row["EmployeeID"]),
                Date = Convert.ToDateTime(row["Date"]),
                CheckInTime = (TimeSpan)row["CheckInTime"],
                CheckOutTime = row["CheckOutTime"] != DBNull.Value ? (TimeSpan?)row["CheckOutTime"] : null
            };
        }
    }
} 