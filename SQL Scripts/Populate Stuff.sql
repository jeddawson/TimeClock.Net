
INSERT INTO timeclock.dbo.PayType (DailyMax, WeeklyMax, Description)
VALUES (24, 168, 'Overtime')
GO
INSERT INTO timeclock.dbo.Holiday (Date, Description, Repeats)
VALUES (02-09-2012, 'TestHoliday', 0);
GO

INSERT INTO timeclock.dbo.Company (Name)
VALUES	('TestCompany')
GO

INSERT INTO timeclock.dbo.Department (Company_CompanyID, PayPeriodInterval, PayPeriodSeed)
VALUES (1, 14, 2012-05-09)
GO

INSERT INTO Employee (EmployeeID, FirstName, LastName, DepartmentID, Terminated, EmployeeID)
VALUES('10-MKRINGELBACH', 'Mikkel', 'Kringelbach', 1, 0, '10-MKRINGELBACH')
GO

INSERT INTO PunchType (Description)
VALUES('Normal')
GO

INSERT INTO PunchType (Description)
VALUES('Sick')
GO

INSERT INTO PunchType (Description)
VALUES('Vacation')
GO

INSERT INTO Punch (EmployeeID, InTime, OutTime, DepartmentID, PunchTypeID)
VALUES ('10-MKRINGELBACH', '2012-05-09 8:00', '2012-05-09 12:30', 1, 1)
GO

INSERT INTO Punch (EmployeeID, InTime, OutTime, DepartmentID, PunchTypeID)
VALUES ('10-MKRINGELBACH', '2012-05-09 13:00', '2012-05-09 17:30', 1, 1)
GO

INSERT INTO Timecard (EmployeeID, PayPeriod)
VALUES ('10-MKRINGELBACH', 1)
GO

INSERT INTO Line (PunchID, PayTypeID, SplitStart, SplitEnd, TimecardID)
VALUES (1, 2, '2012-05-09 8:00', '2012-05-09 12:30', 1)
GO

INSERT INTO Line (PunchID, PayTypeID, SplitStart, SplitEnd, TimecardID)
VALUES (2, 2, '2012-05-09 13:00', '2012-05-09 17:30', 1)
GO

