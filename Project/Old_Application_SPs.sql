USE [PunchTime]
GO
/****** Object:  StoredProcedure [dbo].[web_whosIn]    Script Date: 06/10/2012 23:35:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[web_whosIn]
	@user int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
		
	SELECT	CONVERT(char(10), max(punchDay), 1) as 'date'
			, f_name + ' ' + l_name as 'name'
			, 'In' as 'status'
			, CONVERT(char(5), max(inPunch), 108) as 'inPunch'
			, cast( (DATEDIFF(MI, max(inPunch), convert( time, getdate() ) ) / 60 ) as varchar) + ':' +
				case 
						when DATEDIFF(MI, max(inPunch), convert(time, getdate() ) )%60 < 10 then '0' else ''
				end +
				convert(varchar(2),DATEDIFF(MI, max(inPunch), convert(time, getdate() ) )%60)
		as 'hours'
		FROM timeCard 
		join employee on timeCard.empId = employee.e_id
		where punchDay = CONVERT(DATE, GETDATE(),1) AND (outPunch is null)
		group by timeCard.empId, f_name, l_name
	
	UNION
	
	SELECT	CONVERT(char(10), max(punchDay), 1) as 'date'
			, f_name + ' ' + l_name as 'name'
			, 'Out'		as 'status'
			, '--:--'	as 'inPunch'
			, '--:--'	as 'hours'
		FROM timeCard 
		join employee on timeCard.empId = employee.e_id
		where ( punchDay = CONVERT(DATE, GETDATE(),1) AND (outPunch is not null) )
		OR (timeCard.empId not in (select empId from timeCard as t2 
									where punchDay = CONVERT(DATE, GETDATE(),1))
			AND employee.status = 'Active' )
		group by timeCard.empId, f_name, l_name
		
	
END
GO
/****** Object:  StoredProcedure [dbo].[web_empList]    Script Date: 06/10/2012 23:35:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[web_empList]
	-- NO PARAMS
AS
BEGIN
	SET NOCOUNT ON;
	
	
	select e_id as 'empId', l_name + ', ' + f_name as 'name'from employee
		where status = 'Active'
	order by sort_key DESC, name
	
END
GO
/****** Object:  StoredProcedure [dbo].[web_doPunch]    Script Date: 06/10/2012 23:35:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[web_doPunch]
	-- Add the parameters for the stored procedure here
	@EMP_ID VARCHAR(50),
	@EMP_PIN VARCHAR(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Check to see if pin matches DB
    
    IF @EMP_PIN = (SELECT PIN FROM employee WHERE e_id = @EMP_ID)
		BEGIN
			declare @cycleSeed varchar(8), @cycleLen int, @payPeriod Int, @cpy Int, @dep Int
			set @cycleSeed = (Select value from configuration where setting = 'Cycle Starts')
			set	@cycleLen = (select value from configuration where setting = 'Pay Cycle')
			set @payPeriod = (DATEDIFF(dd,@cycleSeed,GETDATE())/@cycleLen)
			select @cpy = cpy_id, @dep = dep_id from employee where e_id = @EMP_ID
			
			UPDATE employee SET last_date = GETDATE() WHERE e_id = @EMP_ID
			
			IF (select COUNT(*) from timeCard where empId = @EMP_ID AND punchDay = CONVERT(DATE, GETDATE(),1) AND (outPunch is null) ) <> 0
					BEGIN
					--MUST BE IN, IS CLOCKING OUT.	
					if ISNULL((select top 1 DATEDIFF(ss,inPunch, CONVERT(time, getdate(),8)) from timeCard 
									where empId = @EMP_ID and punchDay = CONVERT(DATE, GETDATE(),1) AND (outPunch is null) order by tcId desc),10) > 5 
						UPDATE timeCard
							SET outPunch = CONVERT(time, getdate(),8)
							WHERE empId = @EMP_ID AND punchDay = CONVERT(DATE, GETDATE(),1) AND (outPunch is null)
					END
				ELSE
					BEGIN
					--MUST BE CLOCKING IN.
					if ISNULL((select top 1 DATEDIFF(ss,outPunch, CONVERT(time, getdate(),8)) from timeCard 
									where empId = @EMP_ID and punchDay = CONVERT(DATE, GETDATE(),1) order by tcId desc),10) > 5 
						insert into timeCard (empId, punchDay, payPeriod, inPunch, cpy_id, dept_id)
								values (@EMP_ID, CONVERT(date,getdate(),1), @payPeriod, CONVERT(time, getdate(), 8), @cpy, @dep)
					END
				
			select top 1 tcId as 'success' from timeCard where empId = @EMP_ID order by tcId desc
			
			insert into clockLog (log_entry, log_user)
				values ('Punch', @EMP_ID)
			
		END
	ELSE
		BEGIN
		select top 1 tcId as 'failure' from timeCard where empId = @EMP_ID order by tcId desc
		
		insert into clockLog (log_entry, log_user)
				values ('Failed Punch' + @EMP_PIN , @EMP_ID)
		
		END
    
    /*
	IF @EMP_PIN = (SELECT PIN FROM employee WHERE e_id = @EMP_ID)
		BEGIN
			declare @cycleSeed varchar(8), @cycleLen int, @payPeriod Int
			set @cycleSeed = (Select value from configuration where setting = 'Cycle Starts')
			set	@cycleLen = (select value from configuration where setting = 'Pay Cycle')
			set @payPeriod = (DATEDIFF(dd,@cycleSeed,GETDATE())/@cycleLen)
			
			--prevent duplicat punches
			if (select top 1 datediff(ss, activity.punchTime, GETDATE()) from activity where e_id = @EMP_ID order by punchTime DESC) < 5
				begin
				select top 1 act_id as 'success' from activity where e_id = @EMP_ID order by ACT_ID desc
				
				insert into clockLog (log_entry, log_user)
					values ('Duplicate Logon', @EMP_ID)
				
				return
			end
			
			--Check for missed punches.
			/*Declare @lastDate as date
			Set @lastDate = CONVERT(date, (select last_date from employee where e_id = @EMP_ID), 1)
			
			if @lastDate < CONVERT(date, getDate(), 1)
				Begin
				if (select COUNT(*) from activity where e_id =@EMP_ID and CONVERT(date, activity.punchTime, 1) = @lastDate) % 2 > 0
					Begin
					-- Odd number of punches on prior date of activity. Indicates missed punch.
					SET @NOTICE = 'Missed punch detected for ' + convert(varchar(8),@lastDate, 1)
					End
				End*/
					
			UPDATE employee SET last_date = GETDATE() WHERE e_id = @EMP_ID
			IF (select COUNT(*) from timeCard where empId = @EMP_ID AND punchDay = CONVERT(DATE, GETDATE(),1) AND (outPunch is null) ) <> 0
					BEGIN
					--MUST BE IN, IS CLOCKING OUT.	
					UPDATE timeCard
						SET outPunch = CONVERT(time, getdate(),8)
						WHERE empId = @EMP_ID AND punchDay = CONVERT(DATE, GETDATE(),1) AND (outPunch is null)
					END
				ELSE
					BEGIN
					--MUST BE CLOCKING IN.
					insert into timeCard (empId, punchDay, payPeriod, inPunch)
							values (@EMP_ID, CONVERT(date,getdate(),1), @payPeriod, CONVERT(time, getdate(), 8))
					END
			/*		
			--Check for existing punches
			if (select COUNT(*) from activity where e_id = @EMP_ID) > 0
				Begin
				--See if there are punches for today
				if (select COUNT(*) from activity where e_id = @EMP_ID and
					convert(date,activity.punchTime,1) = CONVERT(date, GETDATE(), 1) ) > 0
					Begin
					--See if the last punch is an 'In'
					if (select top 1 isnull(punchAction, '') from activity where e_id = @EMP_ID order by punchTime DESC) = 'In'
						begin
						insert into activity (punchTime, e_id, punchAction)
							values (getdate(), @emp_id, 'Out')
						update timeCard
							set outPunch = CONVERT(time, getdate(),8)
							where empId = @EMP_ID and tcId = (select top 1 tcId from timeCard as t2 where t2.empId = @EMP_ID order by t2.tcId DESC)
						end
					else
						begin
						--
						insert into activity (punchTime, e_id, punchAction)
							values (getdate(), @emp_id, 'In')
						insert into timeCard (empId, punchDay, payPeriod, inPunch)
							values (@EMP_ID, CONVERT(date,getdate(),1), @payPeriod, CONVERT(time, getdate(), 8))
						end
					end
				--No punches for today, this must be clockin.
				--Should check time to see if missed punch is likely.
				else
					begin
					insert into activity (punchTime, e_id, punchAction)
							values (getdate(), @emp_id, 'In')
					insert into timeCard (empId, punchDay, payPeriod, inPunch)
							values (@EMP_ID, CONVERT(date,getdate(),1), @payPeriod, CONVERT(time, getdate(), 8))
					end
				
				End
			--Since there are no existing punches, punch In. This is the first punch in the system for a new user.
			Else
				Begin
				insert into activity (punchTime, e_id, punchAction)
					values (getdate(), @emp_id, 'In')
				insert into timeCard (empId, punchDay, payPeriod, inPunch)
					values (@EMP_ID, CONVERT(date,getdate(),1), @payPeriod, CONVERT(time, getdate(), 8))
				end*/
			
			select top 1 tcId as 'success' from timeCard where empId = @EMP_ID order by tcId desc
			
			insert into clockLog (log_entry, log_user)
				values ('Punch', @EMP_ID)
			
		END
	ELSE
		BEGIN
		select top 1 tcId as 'failure' from timeCard where empId = @EMP_ID order by tcId desc
		
		insert into clockLog (log_entry, log_user)
				values ('Failed Punch' + @EMP_PIN , @EMP_ID)
		
		END*/
		
END
GO
/****** Object:  StoredProcedure [dbo].[web_deleteEntry]    Script Date: 06/10/2012 23:35:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[web_deleteEntry]
	-- Add the parameters for the stored procedure here
	@empId int, 
	@tcId int,
	@user int
AS
BEGIN

	SET NOCOUNT ON;

	delete from timeCard
		   where empId = @empId and tcId = @tcId
	
	insert into clockLog (log_entry)
		values ('Time card entry ' + CONVERT(varchar(10), @tcId)  + ' deleted by management.')
	
	select top 1 tcId, CONVERT(varchar(8), punchDay, 1) as 'date', inPunch, outPunch from timeCard where empId = @empId order by tcId desc
    
END
GO
/****** Object:  StoredProcedure [dbo].[web_createEntry]    Script Date: 06/10/2012 23:35:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[web_createEntry]
	-- Add the parameters for the stored procedure here
	@empId int, 
	@date varchar(8),
	@inPunch varchar(50),
	@outPunch varchar(50),
	@user int
AS
BEGIN

	SET NOCOUNT ON;

	declare @payPeriod int, @cycleSeed varchar(8), @cycleLen int
	set @cycleSeed = (Select value from configuration where setting = 'Cycle Starts')
	set	@cycleLen = (select value from configuration where setting = 'Pay Cycle')	
	set @payPeriod = (DATEDIFF(dd,@cycleSeed,@date)/@cycleLen)
	
	select @outPunch = case when @outPunch = '' then null else @outPunch end

	insert into timeCard (empId, punchDay, payPeriod, inPunch, outPunch)
		   values (@empId, @date, @payPeriod, @inPunch, @outPunch)
	
	insert into clockLog (log_entry)
		values ('Time card entry create by management.')
	
	select top 1 tcId, CONVERT(varchar(8), punchDay, 1) as 'date', inPunch, outPunch from timeCard where empId = @empId order by tcId desc
    
END
GO
/****** Object:  StoredProcedure [dbo].[web_authenticate]    Script Date: 06/10/2012 23:35:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create PROCEDURE [dbo].[web_authenticate] 
	@uName varchar(255),
	@uPass varchar(255),
	@uIp varchar(12)
AS
BEGIN
	SET NOCOUNT ON;
	
	Declare @passHash varbinary(16)
	
	Select @passHash = HASHBYTES('MD5', @uPass)
	
	if( select COUNT(*) from WEB_USR where uName = @uName and uPass = @passHash ) = 1
		Begin
		
			if (select active from WEB_USR where uName = @uName and uPass = @passHash) = 1
				begin
			
					update WEB_USR
						set lastDt = GETDATE(),
							uniqueId = NEWID(),
							invalidCount = 0
						where uName = @uName and uPass = @passHash
					
					select 'success' as 'status', uniqueId from WEB_USR where uName = @uName and uPass = @passHash
					
					return
				end
			else
				select 'disabled' as 'status'
			
		End
	else
		Begin
		
			if( select COUNT(*) from WEB_USR where uName = @uName) <> 0
				begin
					update WEB_USR
						set invalidCount = invalidCount + 1
						where uName =@uname
					if (select invalidCount from WEB_USR where uName = @uName) > 5
						begin
							update WEB_USR
								set active = 0
								where uName = @uName
							
							select 'locked out' as 'status'
							return
						end
					select 'invalid' as 'status'
				end
		End
END
GO
/****** Object:  StoredProcedure [dbo].[authenticate_employee]    Script Date: 06/10/2012 23:35:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[authenticate_employee]
	-- Add the parameters for the stored procedure here
	@EMP_ID VARCHAR(50),
	@EMP_PIN VARCHAR(50),
	@NOTICE VARCHAR(100) OUTPUT,
	@IS_MANAGER BIT OUTPUT,
	@IS_ADMIN BIT OUTPUT,
	@RESULT INT OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Check to see if pin matches DB
    
	IF @EMP_PIN = (SELECT PIN FROM employee WHERE e_id = @EMP_ID)
		BEGIN
			declare @cycleSeed varchar(8), @cycleLen int, @payPeriod Int
			set @cycleSeed = (Select value from configuration where setting = 'Cycle Starts')
			set	@cycleLen = (select value from configuration where setting = 'Pay Cycle')
			set @payPeriod = (DATEDIFF(dd,@cycleSeed,GETDATE())/@cycleLen)
			
			--prevent duplicat punches
			if (select top 1 datediff(ss, activity.punchTime, GETDATE()) from activity where e_id = @EMP_ID order by punchTime DESC) < 5
				begin
				SET @RESULT = 1
				SET @IS_ADMIN = (SELECT ISNULL(is_admin,0) FROM employee WHERE e_id = @EMP_ID)
				SET @IS_MANAGER = (SELECT ISNULL(is_manager, 0) FROM employee WHERE e_id = @EMP_ID)
				
				insert into clockLog (log_entry, log_user)
					values ('Duplicate Logon', @EMP_ID)
				
				return
			end
			
			--Check for missed punches.
			Declare @lastDate as date
			Set @lastDate = CONVERT(date, (select last_date from employee where e_id = @EMP_ID), 1)
			
			if @lastDate < CONVERT(date, getDate(), 1)
				Begin
				if (select COUNT(*) from activity where e_id =@EMP_ID and CONVERT(date, activity.punchTime, 1) = @lastDate) % 2 > 0
					Begin
					-- Odd number of punches on prior date of activity. Indicates missed punch.
					SET @NOTICE = 'Missed punch detected for ' + convert(varchar(8),@lastDate, 1)
					End
				End
					
			UPDATE employee SET last_date = GETDATE() WHERE e_id = @EMP_ID
			
			--Check for existing punches
			if (select COUNT(*) from activity where e_id = @EMP_ID) > 0
				Begin
				--See if there are punches for today
				if (select COUNT(*) from activity where e_id = @EMP_ID and
					convert(date,activity.punchTime,1) = CONVERT(date, GETDATE(), 1) ) > 0
					Begin
					--See if the last punch is an 'In'
					if (select top 1 isnull(punchAction, '') from activity where e_id = @EMP_ID order by punchTime DESC) = 'In'
						begin
						insert into activity (punchTime, e_id, punchAction)
							values (getdate(), @emp_id, 'Out')
						update timeCard
							set outPunch = CONVERT(time, getdate(),8)
							where empId = @EMP_ID and tcId = (select top 1 tcId from timeCard as t2 where t2.empId = @EMP_ID order by t2.tcId DESC)
						end
					else
						begin
						--
						insert into activity (punchTime, e_id, punchAction)
							values (getdate(), @emp_id, 'In')
						insert into timeCard (empId, punchDay, payPeriod, inPunch)
							values (@EMP_ID, CONVERT(date,getdate(),1), @payPeriod, CONVERT(time, getdate(), 8))
						end
					end
				--No punches for today, this must be clockin.
				--Should check time to see if missed punch is likely.
				else
					begin
					insert into activity (punchTime, e_id, punchAction)
							values (getdate(), @emp_id, 'In')
					insert into timeCard (empId, punchDay, payPeriod, inPunch)
							values (@EMP_ID, CONVERT(date,getdate(),1), @payPeriod, CONVERT(time, getdate(), 8))
					end
				
				End
			--Since there are no existing punches, punch In. This is the first punch in the system for a new user.
			Else
				Begin
				insert into activity (punchTime, e_id, punchAction)
					values (getdate(), @emp_id, 'In')
				insert into timeCard (empId, punchDay, payPeriod, inPunch)
					values (@EMP_ID, CONVERT(date,getdate(),1), @payPeriod, CONVERT(time, getdate(), 8))
				end
			
			SET @RESULT = 1
			SET @IS_ADMIN = (SELECT ISNULL(is_admin,0) FROM employee WHERE e_id = @EMP_ID)
			SET @IS_MANAGER = (SELECT ISNULL(is_manager, 0) FROM employee WHERE e_id = @EMP_ID)
			
			insert into clockLog (log_entry, log_user)
				values ('User Logon', @EMP_ID)
			
		END
	ELSE
		BEGIN
		SET @RESULT = 0
		SET @IS_ADMIN = 0
		SET @IS_MANAGER = 0
		
		insert into clockLog (log_entry, log_user)
				values ('Failed Logon' + @EMP_PIN , @EMP_ID)
		
		END
		
END
GO
/****** Object:  StoredProcedure [dbo].[admin_getOneEmployee]    Script Date: 06/10/2012 23:35:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[admin_getOneEmployee]
	-- Add the parameters for the stored procedure here
	@empId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	IF ISNULL((SELECT cpy_id from employee where e_id = @empId), 0) = 0
		begin
		update employee set cpy_id = 1 where e_id = @empId
		end
	IF ISNULL((SELECT dep_id from employee where e_id = @empId), 0) = 0
		begin
		update employee set dep_id = 1 where e_id = @empId
		end
	
	SELECT e_id as 'Employee Id', pin as 'Pin', RTRIM(employee.status) as 'Status' ,f_name + ' ' + l_name as 'Name', RTRIM(employee.type) as 'Type',
			CASE is_manager
				WHEN 1 THEN 'Yes'
				WHEN 0 THEN 'No'
				WHEN NULL THEN 'No'
			END as 'Manager',
			CASE is_admin
				WHEN 1 THEN 'Yes'
				WHEN 0 THEN 'No'
				WHEN NULL THEN 'No'
			END as 'Admin',
			ISNULL(employee.export,'') as 'Export', ISNULL(company.name,'') as 'Company', ISNULL(department.name,'') as 'Department'
		FROM employee
		join company on employee.cpy_id = company.cpy_id
		join department on employee.dep_id = department.dept_id 
		WHERE employee.e_id = @empId
    
END
GO
/****** Object:  StoredProcedure [dbo].[admin_getEmployees]    Script Date: 06/10/2012 23:35:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[admin_getEmployees]
	-- Add the parameters for the stored procedure here
	@activeOnly varchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	if @activeOnly = '1'
		begin
		SELECT e_id, pin, RTRIM(employee.status) ,f_name + ' ' + l_name, RTRIM(employee.type),
			CASE is_manager
				WHEN 1 THEN 'Yes'
				WHEN 0 THEN 'No'
				WHEN NULL THEN 'No'
			END,
			CASE is_admin
				WHEN 1 THEN 'Yes'
				WHEN 0 THEN 'No'
				WHEN NULL THEN 'No'
			END
		FROM employee 
		WHERE employee.status = 'Active'
		
		return
		end
	else
		begin
		SELECT e_id, pin, RTRIM(employee.status) ,f_name + ' ' + l_name, RTRIM(employee.type),
			CASE is_manager
				WHEN 1 THEN 'Yes'
				WHEN 0 THEN 'No'
				WHEN NULL THEN 'No'
			END,
			CASE is_admin
				WHEN 1 THEN 'Yes'
				WHEN 0 THEN 'No'
				WHEN NULL THEN 'No'
			END
		FROM employee 
		return
		end
END
GO
/****** Object:  StoredProcedure [dbo].[admin_getCompanies]    Script Date: 06/10/2012 23:35:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[admin_getCompanies]
	-- Add the parameters for the stored procedure here
	@activeOnly int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    if @activeOnly = 1
		begin
		SELECT cpy_id, name, status, export from company where status = 'Active'
		return
		end
	else
		begin
		SELECT cpy_id, name, status, export from company
		return
		end
END
GO
/****** Object:  StoredProcedure [dbo].[admin_editEmployee]    Script Date: 06/10/2012 23:35:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[admin_editEmployee]
	-- Add the parameters for the stored procedure here
	@E_ID int,
	@IP_ADDR varchar(50),
	@fieldName varchar(50),
	@newValue varchar(100),
	@response bit output
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	declare @sql nvarchar(1000)
	set @sql = 'update employee set ' + @fieldName + ' = ''' + @newValue + 
				''' where employee.e_id = ''' + convert(varchar,@E_ID) + ''''
   EXEC (@sql)
   
   insert into clockLog (log_entry, log_ip, log_date)
   values ('Employee ' + convert(varchar, @E_ID) + ' edited', @IP_ADDR, GETDATE())
   
	Set @response = 1
   
END
GO
/****** Object:  StoredProcedure [dbo].[admin_deleteEmployee]    Script Date: 06/10/2012 23:35:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[admin_deleteEmployee]
	-- Add the parameters for the stored procedure here
	@empId int,
	@ip varchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    delete from employee where e_id = @empId
    
    insert into clockLog (log_entry, log_ip, log_date)
    values ('Employe ' + ltrim(CONVERT(varchar(10), @empId)) + 'deleted', @ip, GETDATE())
    
END
GO
/****** Object:  StoredProcedure [dbo].[admin_addEmployee]    Script Date: 06/10/2012 23:35:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[admin_addEmployee]
	-- Add the parameters for the stored procedure here
	@E_ID int,
	@IP_ADDR varchar(50),
	@NAME_F VARCHAR(100),
	@NAME_L VARCHAR(100),
	@STATUS VARCHAR(50),
	@PIN VARCHAR(10),
	@TYPE VARCHAR(50),
	@MANAGER VARCHAR(10),
	@ADMIN VARCHAR(10),
	@NEW_EMP bit output
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	insert into employee (f_name, l_name, status, pin, type, is_manager, is_admin)
	values (@NAME_F, @NAME_L, @STATUS, @PIN, @TYPE, @MANAGER, @ADMIN)
	
	insert into clockLog (log_entry, log_ip, log_user, log_date)
	values ('New employee Added', @IP_ADDR, @E_ID, GETDATE())

	set @NEW_EMP = (select top 1 e_id from employee order by e_id desc)
   
END
GO
/****** Object:  StoredProcedure [dbo].[manage_whosIn]    Script Date: 06/10/2012 23:35:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[manage_whosIn]
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	select CONVERT(varchar(8),activity.punchTime,1) as 'Date' , f_name + ' ' + l_name as 'Name',
		activity.punchAction as 'State', CONVERT(varchar(5), activity.punchTime, 108)as 'Time In', 
		convert(varchar(2),DATEDIFF(MI, activity.punchTime, getdate())/60) + ':' +
		case 
			when DATEDIFF(MI, activity.punchTime, getdate())%60 < 10 then '0' else ''
		end +
		convert(varchar(2),DATEDIFF(MI, activity.punchTime, getdate())%60)
		as 'Total'
		from employee
		join activity on employee.e_id = activity.e_id
		where activity.ACT_ID = (select top 1 activity.ACT_ID from activity where activity.e_id = employee.e_id order by activity.punchTime DESC)
		and activity.punchAction = 'In'
END
GO
/****** Object:  StoredProcedure [dbo].[manage_getPunchDetails]    Script Date: 06/10/2012 23:35:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[manage_getPunchDetails]
	-- Add the parameters for the stored procedure here
	@actIn int,
	@actOut int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT ACT_ID, punchTime, punchAction
	from activity where ACT_ID in (@actIn, @actOut)
	
END
GO
/****** Object:  StoredProcedure [dbo].[manage_editPunch]    Script Date: 06/10/2012 23:35:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[manage_editPunch]
	-- Add the parameters for the stored procedure here
	@E_ID int,
	@IP_ADDR varchar(50),
	@actId int,
	@fieldName varchar(50),
	@newValue varchar(100),
	@response bit output
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	declare @sql nvarchar(1000)
	set @sql = 'update activity set ' + @fieldName + ' = ''' + @newValue + 
				''' where activity.act_id = ''' + convert(varchar,@actId) + ''''
   EXEC (@sql)
   
   insert into clockLog (log_entry, log_ip)
   values ('Activity for emp: ' + convert(varchar, @E_ID) + ' edited', @IP_ADDR)
   
	Set @response = 1
END
GO
/****** Object:  StoredProcedure [dbo].[employee_timeCard]    Script Date: 06/10/2012 23:35:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[employee_timeCard] 
	-- Add the parameters for the stored procedure here
	@empId int,
	@date varchar(10)
AS
BEGIN

	SET NOCOUNT ON;
	
	declare @periodStart varchar(8), @periodEnd varchar(8)
	declare @cycleSeed varchar(8), @cycleLen int
	
	set @cycleSeed = (Select value from configuration where setting = 'Cycle Starts')
	set	@cycleLen = (select value from configuration where setting = 'Pay Cycle')
	
	/*update #timeCard set duration = convert(varchar(2),DATEDIFF(MI, #timeCard.clockIn, #timeCard.clockOut)/60) + ':' +
		convert(varchar(2),DATEDIFF(MI, #timeCard.clockIn, #timeCard.clockOut)%60)
	update #timeCard set totalMin = DATEDIFF(MI, #timeCard.clockIn, #timeCard.clockOut) 
	update #timeCard set ot2Min =	case
										when totalMin > 720 then totalMin - 720 else 0
									end
	update #timeCard set ot1Min =	case
										when totalMin > 480 and totalMin <= 720 then totalMin - 480
										when totalMin > 720 then 240 else 0
									end
	update #timeCard set regMin =	case
										when totalMin > 480 then 480 else totalMin
									end

	convert(varchar(2),DATEDIFF(MI, inPunch, outPunch)/60) + ':' +
		CONVERT(varchar(2),datediff(mi, inPunch, outPunch)%60)

	*/
	--Finally dump the results back to web app
	declare @payPeriod Int
	set @payPeriod = (DATEDIFF(dd,@cycleSeed,@date)/@cycleLen)
	
	select punchDay as 'Date', convert(varchar(5),inPunch) as 'In Punch', convert(varchar(5),outPunch) as 'Out Punch',
		convert(varchar(2),DATEDIFF(MI, inPunch, outPunch)/60) + ':' +
		CONVERT(varchar(2),datediff(mi, inPunch, outPunch)%60) as 'Entry Time', 
		(select cast(SUM(reg)/60 as decimal(4,2)) + cast((cast( SUM(reg)%60 as decimal(4,2)) / 60) as decimal(4,2))
			from (	select case when sum(DATEDIFF(MI, t2.inPunch, t2.outPunch)) > 480 then 480
								else sum(DATEDIFF(MI, t2.inPunch, t2.outPunch)) end as 'reg'
					from timeCard as t2 where t2.empId = @empId and t2.payPeriod = @payPeriod
					and t2.tcId <= timeCard.tcId group by t2.punchDay) as A) as 'Reg Hours',
		(select cast(SUM(ot)/60 as decimal(4,2)) + cast((cast( SUM(ot)%60 as decimal(4,2)) / 60) as decimal(4,2))
			from (	select case when sum(DATEDIFF(MI, t2.inPunch, t2.outPunch)) > 480 and sum(DATEDIFF(MI, t2.inPunch, t2.outPunch)) <=720 then sum(DATEDIFF(MI, t2.inPunch, t2.outPunch)) - 480
									when sum(DATEDIFF(MI, t2.inPunch, t2.outPunch)) > 720 then 240
									else 0 end  as 'ot'
					from timeCard as t2 where t2.empId = @empId and t2.payPeriod = @payPeriod
					and t2.tcId <= timeCard.tcId group by t2.punchDay) as A) as 'OT Hours',
		(select CONVERT(varchar(2),sum(dot)/60) + ':' + CONVERT(varchar(2), sum(dot)%60)
			from ( select case	when sum(DATEDIFF(MI, t2.inPunch, t2.outPunch)) > 720 then sum(DATEDIFF(MI, t2.inPunch, t2.outPunch)) - 720
								else 0 end as 'dot'
					from timeCard as t2 where t2.empId = @empId and t2.payPeriod = @payPeriod
					and t2.tcId <= timeCard.tcId group by t2.punchDay) as A) as 'DOT Hours'
		from timeCard where empId = @empId and payPeriod = @payPeriod
		order by punchDay, inPunch
	
END
GO
/****** Object:  StoredProcedure [dbo].[employee_status]    Script Date: 06/10/2012 23:35:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[employee_status]
	-- Add the parameters for the stored procedure here
	@eId int,
	@status varchar(100) out,
	@act_id int out,
	@dep varchar(50) out,
	@exc varchar(50) out,
	@msg varchar(120) out
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	select top 1 @status = punchAction + ' @ ' + CONVERT(varchar(5), punchTime,8) , @act_id = ISNULL(ACT_ID, 0) 
		from activity where e_id = @eId order by punchTime DESC
		
	SET	@dep = (select department.name from employee join department on employee.dep_id = department.dept_id where e_id = @eId)
    SET @exc = ''
    SET @msg = ''
END
GO
/****** Object:  StoredProcedure [dbo].[employee_changePunch]    Script Date: 06/10/2012 23:35:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[employee_changePunch]
	-- Add the parameters for the stored procedure here
	@empId INT, 
	@newPunch VARCHAR(50),
	@actId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	update activity
		set punchAction = @newPunch
		where ACT_ID = @actId
    
    insert into clockLog (log_entry, log_user)
		values ('Employee changed default punchAction to ' +@newPunch, @empId)
    
END
GO
/****** Object:  StoredProcedure [dbo].[web_userAuth]    Script Date: 06/10/2012 23:35:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[web_userAuth]
	-- Add the parameters for the stored procedure here
	@user varchar(255),
	@pass varchar(50),
	@ipAddr varchar(15)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	if (select password from users where username = @user) = @pass
	 Begin
	 
	 update users set
		lastLogin = GETDATE(),
		lastIp = @ipAddr
		where username = @user
	
	 Select groupId, lastReset, 'success' as 'result' from users where username = @user
	 End
	else
	 begin
	 select 'access denied' as 'result' from users
	 end
  
END
GO
/****** Object:  StoredProcedure [dbo].[web_updateTimeEntry]    Script Date: 06/10/2012 23:35:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[web_updateTimeEntry]
	-- Add the parameters for the stored procedure here
	@empId int, 
	@tcId int,
	@inPunch varchar(50),
	@outPunch varchar(50)
AS
BEGIN

	SET NOCOUNT ON;
	
	if @outPunch <> ''
		update timeCard set
		    inPunch = CONVERT(time, @inPunch),
		    outPunch = CONVERT(time, @outPunch)
		    where empId = @empId and tcId = @tcId
	else
		update timeCard set
		    inPunch = CONVERT(time, @inPunch),
		    outPunch = null
		    where empId = @empId and tcId = @tcId
	
	insert into clockLog (log_entry)
		values ('Time card entry num' + cast(@tcId as varchar(50)) + ' modified.')
	
	select tcId, CONVERT(varchar(8), punchDay, 1) as 'date', inPunch, outPunch from timeCard where empId = @empId and tcId = @tcId
    
END
GO
/****** Object:  StoredProcedure [dbo].[web_getTimecardEmployees]    Script Date: 06/10/2012 23:35:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[web_getTimecardEmployees]

AS
BEGIN

	SET NOCOUNT ON;

    select distinct empId, l_name + ', ' + f_name as 'name', sort_key as 'sortId' from timeCard join employee on timeCard.empId = employee.e_id
    order by sort_key, name
    
END
GO
/****** Object:  StoredProcedure [dbo].[web_getTimecard_testing]    Script Date: 06/10/2012 23:35:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[web_getTimecard_testing]
	-- Add the parameters for the stored procedure here
	@empId int = null,
	@payPeriod int = null,
	@user varchar(255) = null
AS
BEGIN

	SET NOCOUNT ON;
	
	if ISNULL(@payPeriod, 0) = 0
		Begin
		declare @cycleSeed varchar(8), @cycleLen int
		set @cycleSeed = (Select value from configuration where setting = 'Cycle Starts')
		set	@cycleLen = (select value from configuration where setting = 'Pay Cycle')
		
		set @payPeriod = (DATEDIFF(dd,@cycleSeed, current_timestamp)/@cycleLen)
		End
	
	SET DATEFIRST 6

	create table #temp
	(
		entryType char(1),
		weekNum int,
		dayNum int,
		punchDay date,
		entryNum int,
		inPunch time,
		outPunch time,
		totalTime int,
		totalHours TIME,
		regTime int,
		overTime int,
		doubleTime int
	)
	
	--insert entries
	insert into #temp (entryType, weekNum, dayNum, punchDay, inPunch, outPunch, totalTime, totalHours)
		select	'p'
				,DATEPART(WK, punchDay)
				,DATEPART(dw, punchDay)
				, punchDay
				, inPunch
				, outPunch
				, DATEDIFF(MI, inPunch, outPunch)
				, CONVERT(varchar(2),DATEDIFF(MI, inPunch, outPunch)/60) + ':' + CONVERT(varchar(2),DATEDIFF(MI, inPunch, outPunch)%60)
		from timeCard where empId = @empId and payPeriod = @payPeriod

	--calc overtime & doubletime
	update #temp
		set regTime = (SELECT CASE WHEN SUM(T2.totalTime) < 480 then #temp.totalTime else 480 - SUM(T2.totalTime) + #temp.totalTime END
						FROM #temp AS T2
						WHERE T2.punchDay = #temp.punchDay AND T2.inPunch <= #temp.inPunch)
			,overTime = (SELECT CASE WHEN SUM(T2.totalTime) > 480 and SUM(T2.totalTime) <= 720 then SUM(T2.totalTime) - 480 
							WHEN SUM(T2.totalTime) > 720 THEN 240
							ELSE 0 END
						FROM #temp AS T2 WHERE T2.inPunch <= #TEMP.inPunch AND T2.punchDay = #TEMP.punchDay )
			,doubleTime =	(SELECT CASE WHEN SUM(T2.totalTime) > 720 THEN SUM(T2.totalTime) - 720
								ELSE 0 END
							FROM #temp AS T2 WHERE T2.inPunch <= #TEMP.inPunch AND T2.punchDay = #TEMP.punchDay )
			, entryNum = (SELECT COUNT(*) FROM #temp AS T2 WHERE T2.inPunch <= #TEMP.inPunch AND T2.punchDay = #TEMP.punchDay)

	--Correct for >40 reg hours in wrk wk
	update #temp
		set	overTime = (SELECT CASE WHEN SUM(T2.regTime) > (40*60) THEN #temp.overTime + (SUM(T2.regTime) - (40*60)) ELSE #temp.overTime END
						FROM #temp AS T2 WHERE T2.weekNum = #temp.weekNum AND (T2.punchDay < #temp.punchDay 
						OR (T2.punchDay = #temp.punchDay AND t2.inPunch <= #temp.inPunch))) 
			,regTime = (SELECT CASE WHEN SUM(T2.regTime) > 2400 THEN #temp.regTime - (SUM(T2.regTime) - 2400) ELSE #temp.regTime END
						FROM #temp AS T2 WHERE T2.weekNum = #temp.weekNum AND ( ( T2.punchDay < #temp.punchDay ) 
						OR (T2.punchDay = #temp.punchDay AND t2.inPunch <= #temp.inPunch))) 

	--insert subtotals
	insert into #temp (entryType, weekNum ,dayNum, punchDay, entryNum, regTime, overTime, doubleTime)
			select	's'
					,DATEPART(wk, punchDay)
					,DATEPART(dw, punchDay)
					, punchDay
					, MAX(entryNum) + 1
					, SUM(regTime)
					, SUM(overTime)
					, SUM(doubleTime)
				from #temp group by punchDay

	--Find missing days
	DECLARE @firstDay date, @lastDay date, @seedDay date, @seedLen int

	select @seedDay = value from configuration where setting = 'Cycle Starts'
	select @seedLen = value from configuration where setting = 'Pay Cycle'
	IF DATEPART(DW,(select MIN(punchDay) from #temp)) <> 1
		select @firstDay = CONVERT(DATE, dateadd(wk, datediff(wk, @seedDay, (select MIN(punchDay) from #temp))-1 , @seedDay))
	ELSE
		select @firstDay = CONVERT(DATE, dateadd(wk, datediff(wk, @seedDay, (select MIN(punchDay) from #temp)), @seedDay))
	select @lastDay = CONVERT(DATE, dateadd(dd, @seedLen-1, @firstDay))
	WHILE (@firstDay <= @lastDay)
		BEGIN
		if (select COUNT(*) from #temp where punchDay = @firstDay) = 0
			insert into #temp (entryType, weekNum, dayNum, punchDay) select 'm', DATEPART(wk, @firstDay), DATEPART(dw, @firstDay), @firstDay
		select @firstDay = DATEADD(dd, 1, @firstDay)	
		END
	
		--check holidays
	IF (select COUNT(*) from #temp where convert(varchar(8),punchDay,1) in (select convert(varchar(8), h_Date, 1) from holidays)) <> 0
		BEGIN
		DECLARE @wkHol int, @dwHol int
		
		update #temp
			set overTime += regTime
				,regTime = 0
				,entryType = 'h'
			where convert(varchar(8),punchDay,1) in (select convert(varchar(8), h_Date, 1) from holidays)
		END
	
	--insert grandtotal
	insert into #temp (entryType, weekNum ,dayNum, punchDay, entryNum, regTime, overTime, doubleTime)
			select	't'
					,DATEPART(wk, MAX(punchDay))
					,DATEPART(dw, MAX(punchDay))
					, MAX(punchDay)
					, MAX(entryNum) + 1
					, SUM(regTime)
					, SUM(overTime)
					, SUM(doubleTime)
				from #temp where inPunch is null and outPunch is null and totalTime is null
	
	--Finally generate data
	select	 ISNULL(entryType, '') as 'eType'
			, ISNULL(weekNum, '') as 'week'
			,convert(varchar(8),punchDay, 1) as 'Date'
			,ISNULL(convert(varchar(5),inPunch), ' ') as 'inPunch'
			,ISNULL(convert(varchar(5),outPunch), ' ') as 'outPunch'
			,ISNULL((CONVERT(varchar(2),totalTime/60) + ':' + CONVERT(varchar(2),totalTime%60)), ' ') as 'entryTime'
			,ISNULL((cast(regTime/60 as decimal(4,2)) + cast((cast( regTime%60 as decimal(4,2)) / 60) as decimal(4,2))), 0) as 'regHours'
			,ISNULL((cast(overTime/60 as decimal(4,2)) + cast((cast( overTime%60 as decimal(4,2)) / 60) as decimal(4,2))), 0)  as 'otHours'
			,ISNULL((cast(doubleTime/60 as decimal(4,2)) + cast((cast( doubleTime%60 as decimal(4,2)) / 60) as decimal(4,2))), 0)  as 'dotHours'
		from #temp
		order by weekNum, dayNum, entryNum
	
	--clean up
	drop table #temp

END
GO
/****** Object:  StoredProcedure [dbo].[web_getTimecard]    Script Date: 06/10/2012 23:35:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[web_getTimecard]
	-- Add the parameters for the stored procedure here
	@empId int = null,
	@payPeriod int = null,
	@user varchar(255) = null
AS
BEGIN

	SET NOCOUNT ON;
	
	if ISNULL(@payPeriod, 0) = 0
		Begin
		declare @cycleSeed varchar(8), @cycleLen int
		set @cycleSeed = (Select value from configuration where setting = 'Cycle Starts')
		set	@cycleLen = (select value from configuration where setting = 'Pay Cycle')
		
		set @payPeriod = (DATEDIFF(dd,@cycleSeed, current_timestamp)/@cycleLen)
		End
	
	SET DATEFIRST 6

	create table #temp
	(
		entryType char(1),
		yearNum	 int,
		weekNum int,
		dayNum int,
		punchDay date,
		entryNum int,
		inPunch time,
		outPunch time,
		totalTime int,
		totalHours TIME,
		regTime int,
		overTime int,
		doubleTime int
	)
	
	--insert entries
	insert into #temp (entryType, yearNum, weekNum, dayNum, punchDay, inPunch, outPunch, totalTime, totalHours)
		select	'p'
				,DATEPART(yy, punchDay)
				,DATEPART(WK, punchDay)
				,DATEPART(dw, punchDay)
				, punchDay
				, inPunch
				, outPunch
				, DATEDIFF(MI, inPunch, outPunch)
				, CONVERT(varchar(2),DATEDIFF(MI, inPunch, outPunch)/60) + ':' + CONVERT(varchar(2),DATEDIFF(MI, inPunch, outPunch)%60)
		from timeCard where empId = @empId and payPeriod = @payPeriod

	--calc overtime & doubletime
	update #temp
		set regTime = (SELECT CASE WHEN SUM(T2.totalTime) < 480 then #temp.totalTime else 480 - SUM(T2.totalTime) + #temp.totalTime END
						FROM #temp AS T2
						WHERE T2.punchDay = #temp.punchDay AND T2.inPunch <= #temp.inPunch)
			,overTime = (SELECT CASE WHEN SUM(T2.totalTime) > 480 and SUM(T2.totalTime) <= 720 then SUM(T2.totalTime) - 480 
							WHEN SUM(T2.totalTime) > 720 THEN 240
							ELSE 0 END
						FROM #temp AS T2 WHERE T2.inPunch <= #TEMP.inPunch AND T2.punchDay = #TEMP.punchDay )
			,doubleTime =	(SELECT CASE WHEN SUM(T2.totalTime) > 720 THEN SUM(T2.totalTime) - 720
								ELSE 0 END
							FROM #temp AS T2 WHERE T2.inPunch <= #TEMP.inPunch AND T2.punchDay = #TEMP.punchDay )
			, entryNum = (SELECT COUNT(*) FROM #temp AS T2 WHERE T2.inPunch <= #TEMP.inPunch AND T2.punchDay = #TEMP.punchDay)

	--Correct for >40 reg hours in wrk wk
	update #temp
		set	overTime = (SELECT CASE WHEN SUM(T2.regTime) > 2400 THEN #temp.overTime + (SUM(T2.regTime) - 2400) ELSE #temp.overTime END
						FROM #temp AS T2 WHERE T2.weekNum = #temp.weekNum AND (T2.punchDay < #temp.punchDay 
						OR (T2.punchDay = #temp.punchDay AND t2.inPunch <= #temp.inPunch))) 
			,regTime = (SELECT CASE WHEN SUM(T2.regTime) > 2400 THEN #temp.regTime - (SUM(T2.regTime) - 2400) ELSE #temp.regTime END
						FROM #temp AS T2 WHERE T2.weekNum = #temp.weekNum AND (T2.punchDay < #temp.punchDay 
						OR (T2.punchDay = #temp.punchDay AND t2.inPunch <= #temp.inPunch))) 

	--insert subtotals
	insert into #temp (entryType, yearNum, weekNum ,dayNum, punchDay, entryNum, regTime, overTime, doubleTime)
			select	's'
					,DATEPART(yy, punchDay)
					,DATEPART(wk, punchDay)
					,DATEPART(dw, punchDay)
					, punchDay
					, MAX(entryNum) + 1
					, SUM(regTime)
					, SUM(overTime)
					, SUM(doubleTime)
				from #temp group by punchDay

	--Find missing days
	DECLARE @firstDay date, @lastDay date, @seedDay date, @seedLen int

	select @seedDay = value from configuration where setting = 'Cycle Starts'
	select @seedLen = value from configuration where setting = 'Pay Cycle'
	IF DATEPART(DW,(select MIN(punchDay) from #temp)) <> 1
		select @firstDay = CONVERT(DATE, dateadd(wk, datediff(wk, @seedDay, (select MIN(punchDay) from #temp))-1 , @seedDay))
	ELSE
		select @firstDay = CONVERT(DATE, dateadd(wk, datediff(wk, @seedDay, (select MIN(punchDay) from #temp)), @seedDay))
	select @lastDay = CONVERT(DATE, dateadd(dd, @seedLen-1, @firstDay))
	WHILE (@firstDay <= @lastDay)
		BEGIN
		if (select COUNT(*) from #temp where punchDay = @firstDay) = 0
			insert into #temp (entryType, weekNum, dayNum, punchDay) select 'm', DATEPART(wk, @firstDay), DATEPART(dw, @firstDay), @firstDay
		select @firstDay = DATEADD(dd, 1, @firstDay)	
		END
	
		--check holidays
	IF (select COUNT(*) from #temp where convert(varchar(8),punchDay,1) in (select convert(varchar(8), h_Date, 1) from holidays)) <> 0
		BEGIN
		DECLARE @wkHol int, @dwHol int
		
		update #temp
			set overTime += regTime
				,regTime = 0
				,entryType = 'h'
			where convert(varchar(8),punchDay,1) in (select convert(varchar(8), h_Date, 1) from holidays)
		END
	
	--insert grandtotal
	insert into #temp (entryType, yearNum, weekNum ,dayNum, punchDay, entryNum, regTime, overTime, doubleTime)
			select	't'
					,DATEPART(yy, MAX(punchDay))
					,DATEPART(wk, MAX(punchDay))
					,DATEPART(dw, MAX(punchDay))
					, MAX(punchDay)
					, MAX(entryNum) + 1
					, SUM(regTime)
					, SUM(overTime)
					, SUM(doubleTime)
				from #temp where inPunch is null and outPunch is null and totalTime is null
	
	--Finally generate data
	select	 ISNULL(entryType, '') as 'eType'
			,convert(varchar(8),punchDay, 1) as 'Date'
			,ISNULL(convert(varchar(5),inPunch), ' ') as 'inPunch'
			,ISNULL(convert(varchar(5),outPunch), ' ') as 'outPunch'
			,ISNULL((CONVERT(varchar(2),totalTime/60) + ':' + CASE WHEN LEN(CONVERT(varchar(2),totalTime%60)) < 2 THEN '0' ELSE '' END + CONVERT(varchar(2),totalTime%60) ), ' ') as 'entryTime'
			,ISNULL((cast(regTime/60 as decimal(4,2)) + cast((cast( regTime%60 as decimal(4,2)) / 60) as decimal(4,2))), 0) as 'regHours'
			,ISNULL((cast(overTime/60 as decimal(4,2)) + cast((cast( overTime%60 as decimal(4,2)) / 60) as decimal(4,2))), 0)  as 'otHours'
			,ISNULL((cast(doubleTime/60 as decimal(4,2)) + cast((cast( doubleTime%60 as decimal(4,2)) / 60) as decimal(4,2))), 0)  as 'dotHours'
		from #temp
		order by yearNum, weekNum, dayNum, entryNum
	
	--clean up
	drop table #temp

END
GO
/****** Object:  StoredProcedure [dbo].[web_getStatus]    Script Date: 06/10/2012 23:35:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[web_getStatus]
	-- Add the parameters for the stored procedure here
	@empId int,
	@pin varchar(50) = null
AS
BEGIN
	SET NOCOUNT ON;
	-- test to see if pin is supplied
	if @pin is not null
		Begin
			if( select pin from employee where e_id = @empId ) not like @pin
				Begin
					select e_id as 'empId', @pin as 'invalid' from employee where e_id = @empId
					return
				End
		End
	
	--New way of doing this with just the timeCard table
	IF (select COUNT(*) from timeCard where empId = @empId AND punchDay = CONVERT(DATE, GETDATE(),1) AND (outPunch is null) ) <> 0
		BEGIN
			--MUST BE IN, NEEDS TO CLOCK OUT.	
			SELECT 'In' as 'lastPunch', 'In' as 'status', inPunch as 'time' from timeCard 
				where empId = @empId AND punchDay = CONVERT(DATE, GETDATE(),1) AND (outPunch is null)
		END
	ELSE
		BEGIN
			--MUST BE OUT, NEEDS TO CLOCK IN.
			SELECT 'Out' as 'lastPunch', 'Out' as 'status', 'none' as 'time'
		END
	
END
GO
/****** Object:  StoredProcedure [dbo].[web_getPeriodSummary]    Script Date: 06/10/2012 23:35:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[web_getPeriodSummary]
	-- Add the parameters for the stored procedure here
	@empId int = null,
	@payPeriod int = null,
	@user varchar(255) = null
AS
BEGIN

	SET NOCOUNT ON;
	
	if ISNULL(@payPeriod, 0) = 0
		Begin
		declare @cycleSeed varchar(8), @cycleLen int
		set @cycleSeed = (Select value from configuration where setting = 'Cycle Starts')
		set	@cycleLen = (select value from configuration where setting = 'Pay Cycle')
		
		set @payPeriod = (DATEDIFF(dd,@cycleSeed, current_timestamp)/@cycleLen)
		End
	
	SET DATEFIRST 6

	create table #temp
	(
		entryType char(1),
		yearNum int,
		weekNum int,
		dayNum int,
		punchDay date,
		entryNum int,
		inPunch time,
		outPunch time,
		deptNum int,
		deptName varchar(500),
		totalTime int,
		totalHours TIME,
		regTime int,
		overTime int,
		doubleTime int
	)
	
	--insert entries
	insert into #temp (entryType, yearNum, weekNum, dayNum, punchDay, inPunch, outPunch, deptNum, totalTime, totalHours)
		select	'p'
				,DATEPART(yy, punchDay)
				,DATEPART(WK, punchDay)
				,DATEPART(dw, punchDay)
				, punchDay
				, inPunch
				, outPunch
				, dept_id
				, DATEDIFF(MI, inPunch, outPunch)
				, CONVERT(varchar(2),DATEDIFF(MI, inPunch, outPunch)/60) + ':' + CONVERT(varchar(2),DATEDIFF(MI, inPunch, outPunch)%60)
		from timeCard where empId = @empId and payPeriod = @payPeriod

	--calc overtime & doubletime
	update #temp
		set regTime = (SELECT CASE WHEN SUM(T2.totalTime) < 480 then #temp.totalTime else 480 - SUM(T2.totalTime) + #temp.totalTime END
						FROM #temp AS T2
						WHERE T2.punchDay = #temp.punchDay AND T2.inPunch <= #temp.inPunch)
			,overTime = (SELECT CASE WHEN SUM(T2.totalTime) > 480 and SUM(T2.totalTime) <= 720 then SUM(T2.totalTime) - 480 
							WHEN SUM(T2.totalTime) > 720 THEN 240
							ELSE 0 END
						FROM #temp AS T2 WHERE T2.inPunch <= #TEMP.inPunch AND T2.punchDay = #TEMP.punchDay )
			,doubleTime =	(SELECT CASE WHEN SUM(T2.totalTime) > 720 THEN SUM(T2.totalTime) - 720
								ELSE 0 END
							FROM #temp AS T2 WHERE T2.inPunch <= #TEMP.inPunch AND T2.punchDay = #TEMP.punchDay )
			, entryNum = (SELECT COUNT(*) FROM #temp AS T2 WHERE T2.inPunch <= #TEMP.inPunch AND T2.punchDay = #TEMP.punchDay)

	--Correct for >40 reg hours in wrk wk
	update #temp
		set	overTime = (SELECT CASE WHEN SUM(T2.regTime) > 2400 THEN #temp.overTime + (SUM(T2.regTime) - 2400) ELSE #temp.overTime END
						FROM #temp AS T2 WHERE T2.weekNum = #temp.weekNum AND (T2.punchDay < #temp.punchDay 
						OR (T2.punchDay = #temp.punchDay AND t2.inPunch <= #temp.inPunch))) 
			,regTime = (SELECT CASE WHEN SUM(T2.regTime) > 2400 THEN #temp.regTime - (SUM(T2.regTime) - 2400) ELSE #temp.regTime END
						FROM #temp AS T2 WHERE T2.weekNum = #temp.weekNum AND (T2.punchDay < #temp.punchDay 
						OR (T2.punchDay = #temp.punchDay AND t2.inPunch <= #temp.inPunch))) 
	
	--check holidays
	IF (select COUNT(*) from #temp where convert(varchar(8),punchDay,1) in (select convert(varchar(8), h_Date, 1) from holidays)) <> 0
		BEGIN
		DECLARE @wkHol int, @dwHol int
		
		update #temp
			set overTime += regTime
				,regTime = 0
				,entryType = 'h'
			where convert(varchar(8),punchDay,1) in (select convert(varchar(8), h_Date, 1) from holidays)
		END
		
	--insert subtotals
	insert into #temp (entryType, yearNum, weekNum ,dayNum, punchDay, entryNum, regTime, overTime, doubleTime)
			select	's'
					, DATEPART(yy, punchDay)
					, DATEPART(wk, punchDay)
					, DATEPART(dw, punchDay)
					, punchDay
					, MAX(entryNum) + 1
					, SUM(regTime)
					, SUM(overTime)
					, SUM(doubleTime)
				from #temp where entryType not like 'm' group by punchDay
	
	--insert week1 total
	insert into #temp (entryType, yearNum, weekNum ,dayNum, punchDay, entryNum, regTime, overTime, doubleTime)
			select	'k'
					,DATEPART(yy, MAX(punchDay))
					,DATEPART(wk, MAX(punchDay))
					,DATEPART(dw, MAX(punchDay))
					, MAX(punchDay)
					, MAX(isnull(entryNum, 1))+ 1
					, SUM(regTime)
					, SUM(overTime)
					, SUM(doubleTime)
				from #temp where inPunch is null and outPunch is null and totalTime is null and entryType = 's'
				group by weekNum
	
	--insert week1 total
	insert into #temp (entryType, yearNum, weekNum ,dayNum, punchDay, entryNum, deptNum, regTime, overTime, doubleTime)
			select	'd'
					,DATEPART(yy, max(punchDay))
					,DATEPART(wk, MAX(punchDay))
					,DATEPART(dw, MAX(punchDay))
					, MAX(punchDay)
					, MAX(isnull(entryNum, 1))+ 1
					, deptNum
					, SUM(regTime)
					, SUM(overTime)
					, SUM(doubleTime)
				from #temp where entryType in ('p', 'h')
				group by deptNum
	
	--insert grandtotal
	insert into #temp (entryType, yearNum, weekNum ,dayNum, punchDay, entryNum, regTime, overTime, doubleTime)
			select	't'
					,DATEPART(yy, MAX(punchDay))
					,DATEPART(wk, MAX(punchDay))
					,DATEPART(dw, MAX(punchDay))
					, MAX(punchDay)
					, MAX(isnull(entryNum, 1)) + 1
					, SUM(regTime)
					, SUM(overTime)
					, SUM(doubleTime)
				from #temp where inPunch is null and outPunch is null and totalTime is null and entryType = 's'
	
	update #temp
		set deptName = (select d.name from department as d where d.dept_id = #temp.deptNum ),
			deptNum =  (select d.exportValue from department as d where d.dept_id = #temp.deptNum )
		where deptNum is not null
	
	--Finally generate data
	select	 ISNULL(entryType, '') as 'eType'
			,convert(varchar(8),punchDay, 1) as 'Date'
			,ISNULL(convert(varchar(5),inPunch), ' ') as 'inPunch'
			,ISNULL(convert(varchar(5),outPunch), ' ') as 'outPunch'
			,ISNULL(deptName, '') as 'depName'
			,ISNULL(deptNum, '') as 'depNum'
			,ISNULL((CONVERT(varchar(2),totalTime/60) + ':' + CASE WHEN LEN(CONVERT(varchar(2),totalTime%60)) < 2 THEN '0' ELSE '' END + CONVERT(varchar(2),totalTime%60) ), ' ') as 'entryTime'
			,ISNULL((cast(regTime/60 as decimal(4,2)) + cast((cast( regTime%60 as decimal(4,2)) / 60) as decimal(4,2))), 0) as 'regHours'
			,ISNULL((cast(overTime/60 as decimal(4,2)) + cast((cast( overTime%60 as decimal(4,2)) / 60) as decimal(4,2))), 0)  as 'otHours'
			,ISNULL((cast(doubleTime/60 as decimal(4,2)) + cast((cast( doubleTime%60 as decimal(4,2)) / 60) as decimal(4,2))), 0)  as 'dotHours'
		from #temp
		where entryType in ('p', 'h', 'k', 't', 'd')
		order by yearNum, weekNum, dayNum, entryNum
	
	--clean up
	drop table #temp

END
GO
/****** Object:  StoredProcedure [dbo].[web_getPayPeriods]    Script Date: 06/10/2012 23:35:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[web_getPayPeriods]

AS
BEGIN

	SET NOCOUNT ON;

    --select distinct payPeriod as 'period' from timeCard
    select CONVERT(char(10),MAX(punchDay),1) as 'lastDay', payPeriod as 'period' 
		from timeCard 
		GROUP BY payPeriod
		Order By MAX(punchDay) desc
   
END
GO
/****** Object:  StoredProcedure [dbo].[web_getEntries]    Script Date: 06/10/2012 23:35:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[web_getEntries]
	-- Add the parameters for the stored procedure here
	@empId int, 
	@userId int,
	@period int
AS
BEGIN

	SET NOCOUNT ON;

    SELECT tcId, convert(varchar(8),punchDay, 1) as 'date', inPunch, outPunch, timeCard.dept_id as 'deptId', department.name as 'deptName'
    from timeCard 
    left outer join department on timeCard.dept_id = department.dept_id
    where empId = @empId and payPeriod = @period order by punchDay, inPunch
    
END
GO
/****** Object:  StoredProcedure [dbo].[web_getDeptList]    Script Date: 06/10/2012 23:35:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[web_getDeptList]

AS
BEGIN
	SET NOCOUNT ON;
	
	select dept_id, name
	from department where status = 'Active' order by dept_id
	
END
GO
/****** Object:  StoredProcedure [dbo].[web_get_period_export]    Script Date: 06/10/2012 23:35:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[web_get_period_export]
	-- Add the parameters for the stored procedure here
	@period int
AS
BEGIN
	SET NOCOUNT ON;
	
	if ISNULL(@period, 0) = 0
		Begin
		declare @cycleSeed varchar(8), @cycleLen int
		set @cycleSeed = (Select value from configuration where setting = 'Cycle Starts')
		set	@cycleLen = (select value from configuration where setting = 'Pay Cycle')
		
		set @period = (DATEDIFF(dd,@cycleSeed, current_timestamp)/@cycleLen)
		End
	
	DECLARE emp_cursor CURSOR FOR
		SELECT e_id from employee;
	OPEN emp_cursor;
	
	Declare @EMP_ID INTEGER
	
	FETCH NEXT FROM emp_cursor INTO @EMP_ID
	
	create table #export
	(
		employee_no varchar(12),
		department_no varchar(2),
		earnings_code varchar(2),
		hours_str decimal(4,2)
	)
	
	
	WHILE @@FETCH_STATUS = 0
		BEGIN
		
		create table #results
		(
			eType char(1),
			Date varchar(8),
			inPunch varchar(5),
			outPunch varchar(5),
			depName varchar(500),
			depNum integer,
			entryTime varchar(10),
			regHours decimal(4,2),
			otHours decimal(4,2),
			dotHours decimal(4,2)
		)

		insert #results EXEC web_getPeriodSummary @EMP_ID, @PERIOD, ' '
		
		insert into #export
			select employee.export, depNum, '01', regHours
				from #results
				join employee on employee.e_id = @EMP_ID
				where eType = 'd' and regHours > 0
		
		insert into #export
			select employee.export, depNum, '02', otHours 
				from #results
				join employee on employee.e_id = @EMP_ID
				where eType = 'd' and otHours > 0
			
		insert into #export
			select employee.export, depNum, '03', dotHours 
				from #results
				join employee on employee.e_id = @EMP_ID
				where eType = 'd' and dotHours > 0

		drop table #results
		
		FETCH NEXT FROM emp_cursor INTO @EMP_ID;

		END
	CLOSE emp_cursor;
	DEALLOCATE emp_cursor;
	
	select * from #export
	
	drop table #export

END
GO
