CREATE PROCEDURE sp_GetAttendanceSummary
    @Month INT,
    @Year INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        EmployeeId,
        @Month AS [Month],
        @Year AS [Year],
        SUM(WorkAttendance) AS TotalStandardWorkDays,
        SUM(PaidLeaveAttendance) AS TotalPaidLeaveDays,
        SUM(WorkAttendance + PaidLeaveAttendance) AS TotalSalaryDays,
        SUM(UnpaidLeaveDays) AS TotalUnpaidLeaveDays,
        SUM(OvertimeHours) AS TotalOvertimeHours
    FROM 
        DailyAttendance
    WHERE 
        MONTH([Date]) = @Month AND YEAR([Date]) = @Year
    GROUP BY 
        EmployeeId;
END;