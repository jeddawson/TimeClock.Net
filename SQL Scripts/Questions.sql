

SELECT SUM(DATEDIFF(MINUTE,L.SplitStart,L.SplitEnd)) AS TotalMinutes
FROM timeclock.dbo.Line AS L
WHERE L.TimecardID = (
	SELECT Tc.TimecardID
	FROM timeclock.dbo.Timecard AS Tc
	WHERE Tc.EmployeeID = (
		SELECT E.EmployeeID
		FROM timeclock.dbo.Employee AS E
		WHERE E.EmployeeID = 1234
	)
)
GO

SELECT H.Description
FROM timeclock.dbo.Holiday AS H
WHERE DATEDIFF(DAY, H.Date, GETDATE()) = 0 AND H.HolidayID IN (
	SELECT HD.Holiday_HolidayID AS HolidayID
	FROM timeclock.dbo.HolidayDepartment AS HD
	WHERE HD.Department_DepartmentID IN (
		SELECT D.DepartmentID
		FROM timeclock.dbo.Department AS D
		WHERE D.DepartmentID = 1
		)
	)
GO

SELECT SUM(DATEDIFF(MINUTE,L.SplitStart,L.SplitEnd)) AS TotalOverTimeMinutes
FROM timeclock.dbo.Line AS L
WHERE L.TimecardID = (
	SELECT Tc.TimecardID
	FROM timeclock.dbo.Timecard AS Tc
	WHERE Tc.EmployeeID = (
		SELECT E.EmployeeID
		FROM timeclock.dbo.Employee AS E
		WHERE E.EmployeeID = 1234
		)
	) 
AND L.PayTypeID = (
	SELECT TOP 1 P.PayTypeID
	FROM timeclock.dbo.PayType AS P	
	WHERE P.Description = 'Overtime'
	)
	
GO

SELECT COUNT(P.PunchID) AS SickDaysTaken
FROM Punch AS P
WHERE P.PunchTypeID IN (
	SELECT PT.PunchTypeID
	FROM PunchType AS PT
	WHERE PT.Description = 'Sick'
	) AND
	P.EmployeeID = 1234
GO

--SELECT PT.Description AS NextPayScale
--FROM PayType AS PT
--WHERE PT.NextPayType_PayTypeID IN (
--	SELECT PT1.PayTypeID
--	FROM PayType AS PT1
--	WHERE PT1.Description = 'Normal' 
--	AND PT1.PayTypeID IN (
--		SELECT D.
--		FROM Department AS D
--		)
--	)


SELECT E.EmployeeID
FROM Employee AS E
WHERE E.EmployeeID IN (
	SELECT P.EmployeeID
	FROM Punch AS P
	WHERE P.OutTime
	) 
GO

SELECT E.Em
