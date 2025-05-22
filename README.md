# Employee Attendance System

An ASP.NET Core Web API for tracking employee attendance with role-based access control.

## Features

- **Authentication**: Cookie-based login system with role-based access control
- **Role-based Access**:
  - GeneralEmployee: Can manage their own attendance records
  - Manager: Can view attendance records of their department
  - GeneralManager: Can view all attendance records
  - Administrator: Full system access including employee management
- **Database**: SQL Server with ADO.NET (no Entity Framework)
- **API Documentation**: Swagger UI available in development mode

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQL Server (Local or Remote)
- Visual Studio or other C# IDE

### Database Setup

1. Update the connection string in `appsettings.json` if needed:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=EmployeeAttendanceDB;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```

2. Execute the SQL script in `Data/CreateDatabase.sql` to create the database and tables.

### Running the API

1. Clone the repository
2. Build the solution
3. Run the API project
4. Access Swagger UI at `https://localhost:<port>/swagger`

### Default Users

The system is pre-configured with the following users:

- **Administrator**:
  - Username: admin
  - Password: admin123

- **Department Manager**:
  - Username: manager
  - Password: manager123
  - Department: IT

- **General Manager**:
  - Username: generalmanager
  - Password: manager123

- **General Employee**:
  - Username: employee
  - Password: employee123

## API Endpoints

### Authentication

- `POST /api/auth/login` - Login with username and password
- `POST /api/auth/logout` - Logout current user
- `GET /api/auth/currentuser` - Get current user information

### Employees

- `GET /api/employee` - Get all employees (Admin, GeneralManager) or department employees (DepartmentManager)
- `GET /api/employee/{id}` - Get employee by ID
- `POST /api/employee` - Create new employee (Admin only)
- `PUT /api/employee/{id}` - Update employee (Admin only)
- `DELETE /api/employee/{id}` - Delete employee (Admin only)
- `POST /api/employee/import` - Import employees from file (Admin only)

### Attendance

- `GET /api/attendance` - Get all attendance records (Admin only)
- `GET /api/attendance/department/{department}` - Get department attendance records
- `GET /api/attendance/{employeeId}` - Get employee attendance records
- `GET /api/attendance/{employeeId}/today` - Get employee's attendance for today
- `POST /api/attendance/checkin` - Check in (GeneralEmployee only)
- `POST /api/attendance/checkout` - Check out (GeneralEmployee only)

## Console Client

A separate console application is provided for batch importing employees:

1. Run the console application
2. Login with administrator credentials
3. Choose to import from CSV or JSON file
4. Provide the file path

## License

This project is licensed under the MIT License. 