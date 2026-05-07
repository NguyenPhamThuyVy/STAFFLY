CREATE DATABASE QLNS;
GO
USE QLNS;
GO

-- 1. NHÓM HỆ THỐNG
CREATE TABLE Roles (
    RoleID INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(50) NOT NULL
);

CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) UNIQUE NOT NULL,
    Password NVARCHAR(255) NOT NULL,
    RoleID INT FOREIGN KEY REFERENCES Roles(RoleID),
    EmployeeID INT NULL,
    IsActive BIT DEFAULT 1 
);

CREATE TABLE AuditLogs (
    LogID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT FOREIGN KEY REFERENCES Users(UserID),
    Action NVARCHAR(100),
    Detail NVARCHAR(MAX),
    Timestamp DATETIME DEFAULT GETDATE()
);

-- 2. NHÓM DANH MỤC
CREATE TABLE Departments (
    DepartmentID INT PRIMARY KEY IDENTITY(1,1),
    DepartmentName NVARCHAR(100) NOT NULL,
    HeadcountLimit INT DEFAULT 0,
    CurrentStaffCount INT DEFAULT 0 
);

-- 3. NHÓM NHÂN SỰ
CREATE TABLE Employees (
    EmployeeID INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100),
    Phone NVARCHAR(20),
    Address NVARCHAR(255),
    DateOfBirth DATE,
    DepartmentID INT FOREIGN KEY REFERENCES Departments(DepartmentID),
    Status NVARCHAR(50)
);

CREATE TABLE TransferHistory (
    TransferID INT PRIMARY KEY IDENTITY(1,1),
    EmployeeID INT FOREIGN KEY REFERENCES Employees(EmployeeID),
    FromDepartmentID INT FOREIGN KEY REFERENCES Departments(DepartmentID),
    ToDepartmentID INT FOREIGN KEY REFERENCES Departments(DepartmentID),
    TransferDate DATETIME DEFAULT GETDATE()
);

-- 4. NHÓM VẬN HÀNH
CREATE TABLE Contracts (
    ContractID INT PRIMARY KEY IDENTITY(1,1),
    EmployeeID INT FOREIGN KEY REFERENCES Employees(EmployeeID),
    ContractType NVARCHAR(50),
    SignDate DATE,
    ExpiryDate DATE,
    BasicSalary DECIMAL(18, 2)
);

CREATE TABLE Attendance (
    AttendanceID INT PRIMARY KEY IDENTITY(1,1),
    EmployeeID INT FOREIGN KEY REFERENCES Employees(EmployeeID),
    Date DATE,
    Status NVARCHAR(50)
);

CREATE TABLE Payroll (
    PayrollID INT PRIMARY KEY IDENTITY(1,1),
    EmployeeID INT FOREIGN KEY REFERENCES Employees(EmployeeID),
    Month INT,
    Year INT,
    TotalBonus DECIMAL(18, 2) DEFAULT 0,
    TotalSalary DECIMAL(18, 2) DEFAULT 0,
    Status NVARCHAR(50),
    ApprovedBy INT FOREIGN KEY REFERENCES Users(UserID), 
    ApprovalDate DATETIME 
);