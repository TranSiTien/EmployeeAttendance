-- Create database
IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'EmployeeAttendanceDB')
BEGIN
    CREATE DATABASE EmployeeAttendanceDB;
END
GO

USE EmployeeAttendanceDB;
GO

-- Create Employees table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Employees]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Employees](
        [EmployeeID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [LastName] [nvarchar](50) NOT NULL,
        [FirstName] [nvarchar](50) NOT NULL,
        [Gender] [nvarchar](10) NOT NULL,
        [Department] [nvarchar](50) NOT NULL,
        [PhoneNumber] [nvarchar](20) NOT NULL,
        [IsIntern] [bit] NOT NULL DEFAULT 0,
        [Role] [nvarchar](20) NOT NULL,
        [Band] [nvarchar](20) NOT NULL,
        [TechnicalDirection] [nvarchar](50) NULL,
        [HasCodingSkill] [bit] NOT NULL DEFAULT 0
    )
END
GO

-- Create AttendanceRecords table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AttendanceRecords]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AttendanceRecords](
        [RecordID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [EmployeeID] [int] NOT NULL,
        [Date] [date] NOT NULL,
        [CheckInTime] [time] NOT NULL,
        [CheckOutTime] [time] NULL,
        CONSTRAINT [FK_AttendanceRecords_Employees] FOREIGN KEY([EmployeeID]) 
        REFERENCES [dbo].[Employees] ([EmployeeID])
    )
END
GO

-- Create Users table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Users](
        [UserID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Username] [nvarchar](50) NOT NULL UNIQUE,
        [PasswordHash] [nvarchar](100) NOT NULL,
        [Role] [nvarchar](20) NOT NULL,
        [DepartmentManaged] [nvarchar](50) NULL,
        [IsGeneralManager] [bit] NOT NULL DEFAULT 0
    )
END
GO

-- Insert some initial test users
IF NOT EXISTS (SELECT * FROM [Users] WHERE [Username] = 'admin')
BEGIN
    INSERT INTO [Users] ([Username], [PasswordHash], [Role], [DepartmentManaged], [IsGeneralManager])
    VALUES 
        ('admin', 'AQAAAAIAAYagAAAAELQ3l345NDF99WRRPCXS+1MVpF8TcnDQXSUH1RgbTd0LCvinrWUaFhJQjP+0LX5dSw==', 'Administrator', NULL, 0),
        ('manager', 'AQAAAAIAAYagAAAAELQ3l345NDF99WRRPCXS+1MVpF8TcnDQXSUH1RgbTd0LCvinrWUaFhJQjP+0LX5dSw==', 'Manager', 'IT', 0),
        ('generalmanager', 'AQAAAAIAAYagAAAAELQ3l345NDF99WRRPCXS+1MVpF8TcnDQXSUH1RgbTd0LCvinrWUaFhJQjP+0LX5dSw==', 'Manager', NULL, 1),
        ('employee', 'AQAAAAIAAYagAAAAELQ3l345NDF99WRRPCXS+1MVpF8TcnDQXSUH1RgbTd0LCvinrWUaFhJQjP+0LX5dSw==', 'GeneralEmployee', NULL, 0)
END
GO 