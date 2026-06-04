# STAFFLY: A Human Resource Management System

STAFFLY is an enterprise-grade desktop human resource management software built using **WPF (.NET Framework / .NET Core)** and **Microsoft SQL Server**. Adhering strictly to the **MVVM Architectural Pattern**, the system empowers organizations with secure multi-level payroll calculation pipelines, real-time analytics dashboards, robust administrative logging frameworks, and bulletproof user security.

---

## Technology Stack & Requirements

- **Frontend Interface:** WPF (Windows Presentation Foundation)
- **Architecture Pattern:** MVVM (Model-View-ViewModel) via CommunityToolkit.Mvvm
- **Data Visualization Engine:** LiveCharts.Wpf (PieSeries & ColumnSeries graphs)
- **Database Backend:** Microsoft SQL Server
- **ORM / Data Access Layer:** Entity Framework Core (EF Core)

---

## Key System Features

The STAFFLY system architecture is engineered to provide robust features distributed logically across three distinctive system user roles (Actors):

### 1. System Admin 
- **Manage User Accounts:** Automates profile provisioning, administrative registration, and secure lifecycle maintenance for both HR Manager and HR Staff credentials.
- **Assign Role-Based Access Control (RBAC):** Enforces a multi-level access control matrix ensuring strict operational separation (e.g., restricting payroll validation exclusively to Managers, while limiting input permissions to Staff).
- **Maintain Audit Logs:** Chronicles critical backend data mutations and entry timestamps into untampered system trails, ensuring complete operational transparency.
- **Manage Departments:** Architectures the core departmental organization layout and administrative structural divisions within the master node.

### 2. HR Manager 
- **Approve Payroll & Bonuses:** Executes final fiscal reviews and multi-tier approval authorizations on compiled monthly salary spreadsheets submitted by HR Staff.
- **Process Monthly Attendance:** Audits monthly transactional clock-in logs and evaluates administrative leave requests, lateness patterns, or contextual shift overrides.
- **View Department Reports:** Generates aggregated macro-level insights on business units (e.g., identifying severe department understaffing or high turnover rates) and compiles departmental expenditure metrics to support high-level corporate budget auditing.
- **Review HR Analytics Dashboards (Intelligent Extended Feature):** Aggregates cross-context statistics into intuitive real-time graphical representations tracking workforce metrics and department allocations to support strategic decision-making.
- **Update Departmental Transfer:** Governs internal organizational relocations by dynamically tracking and archiving continuous career history paths, ensuring database headcount reporting always reflects true organizational states.

### 3. HR Staff 
- **Maintain Employee Records:** Coordinates core staff operational profiles, updates profile information parameters, and chronicles contextual contractual adjustment lifecycles.
- **Generate Payroll Reports:** Calculates monthly salary distributions based on attendance logs and bonus entries, executing programmatic data validation checks before submission to the HR Manager.
- **Assign Employees to Departments:** Binds individual employee units to designated departments while actively enforcing headcount allocation locks (`HeadcountLimit` vs `CurrentStaffCount`) to trigger system warnings if a department exceeds its capacity constraints.

---

## Project Structural Directory (MVVM Pattern)

The project workspace layout strictly conforms to software engineering separation of concerns:
```text
Staffly/
│
├── Models/             # Database Entities & Data Structures (EF Core Mappings)
├── ViewModels/         # Application Core State & Presentation Logic
├── Views/              # WPF XAML Visual Interface Layouts & Styles
│   └── Shared/         # Reusable Custom Controls & Core Component UI
│
├── Database/           # DbContext Configurations & Migration Scripts
├── Converters/         # UI Content Value Converters (XAML Data Formatters)
└── App.config          # Core Context Global Configuration Metadata

---

## Local Installation & Database Setup Guide

Follow these sequential deployment instructions to host and initiate the application workspace locally:

### 1. Database Initialization via EF Core
1. Ensure your local instance of **Microsoft SQL Server Management Studio (SSMS)** is online.
2. Open the project solution file using Visual Studio and navigate to the database connection string (located inside `App.config`, `appsettings.json`, or encapsulated within `StafflyDbContext.cs`).
3. Update the connection parameter string to target your local SQL Server instance node:
   ```csharp
   "Server=YOUR_LOCAL_SERVER_NAME;Database=STAFFLY;Trusted_Connection=True;"
4. Access the Package Manager Console inside Visual Studio and issue the database compilation command to deploy the structure onto your SQL Server:
PowerShell
PM> Update-Database
### 2. Solution Compilation and Runtime Hosting
1. Perform a fresh workspace optimization to flush older files and unlock assembly locks (MSB3027 / MSB3021):
- Navigate to the top menu and select Build => Clean Solution
- Navigate to the top menu and select Build => Rebuild Solution
2. Press F5 or click Start to run the STAFFLY desktop environment.
### 3. Operational Prerequisites
Before initializing the monthly attendance workflows or generating structural payroll distributions within the system, the following external data components must be prepared and formatted correctly:
1. **Structured Attendance Log Sheet (`.xlsx`):** The system requires a pre-validated spreadsheet containing core operational columns, specifically: **Employee ID**, **Employee Name**, **Date**, and **Attendance Status (Late/Absent)** to feed the `Process Monthly Attendance` runtime module.
2. **Base Salary & Bonus Matrix:** Pre-validated payroll baseline spreadsheets structured meticulously by **Month** and **Department**. These source files are required by the HR Staff to execute mandatory data validation checks before submitting the compiled sheets to the **HR Manager** for final fiscal approval.

---

## Default System CredentialsFor assessment 
For assessment and local demo validation purposes, you can use the default automated seed profiles below to log into the respective system interface roles:
- System Admin: username: admin // password: 123
- HR Manager: username: manager // password: abc
- HR Staff: username: staff // password: a1b2

---

## Git Branch Convergence Safety Routine
To maintain maximum code stability and prevent repository corruption, always adhere to this collaborative delivery routine when merging work streams into the main product line:
1. Stage and commit local code updates safely on your active branch:
Bash
git add .
git commit -m "Your descriptive change log message"
2. Pull down the latest verified version of the cloud repo:
Bash
git pull origin main
3. Resolve any textual collisions (Merge Conflicts) inside Visual Studio's layout merge grid before executing compilation.
4. Push your compiled, bug-free codebase directly back onto the remote repository:
Bash
git push origin main
