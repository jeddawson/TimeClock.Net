namespace TimeClock.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Employee",
                c => new
                    {
                        EmployeeID = c.String(nullable: false, maxLength: 128),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Terminated = c.Boolean(nullable: false),
                        Pin = c.String(),
                        DepartmentID = c.Int(nullable: false),
                        ManagerID = c.String(),
                        Manager_EmployeeID = c.String(maxLength: 128),
                        Message_MessageID = c.Int(),
                    })
                .PrimaryKey(t => t.EmployeeID)
                .ForeignKey("Employee", t => t.Manager_EmployeeID)
                .ForeignKey("Message", t => t.Message_MessageID)
                .ForeignKey("Department", t => t.DepartmentID, cascadeDelete: true)
                .Index(t => t.Manager_EmployeeID)
                .Index(t => t.Message_MessageID)
                .Index(t => t.DepartmentID);
            
            CreateTable(
                "Punch",
                c => new
                    {
                        PunchID = c.Int(nullable: false, identity: true),
                        InTime = c.DateTime(nullable: false),
                        OutTime = c.DateTime(nullable: false),
                        DepartmentID = c.Int(nullable: false),
                        EmployeeID = c.String(maxLength: 128),
                        PunchTypeID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.PunchID)
                .ForeignKey("Employee", t => t.EmployeeID)
                .ForeignKey("Department", t => t.DepartmentID, cascadeDelete: true)
                .ForeignKey("PunchType", t => t.PunchTypeID, cascadeDelete: true)
                .Index(t => t.EmployeeID)
                .Index(t => t.DepartmentID)
                .Index(t => t.PunchTypeID);
            
            CreateTable(
                "Line",
                c => new
                    {
                        LineID = c.Int(nullable: false, identity: true),
                        PunchID = c.Int(nullable: false),
                        TimecardID = c.Int(nullable: false),
                        PayTypeID = c.Int(nullable: false),
                        SplitStart = c.DateTime(nullable: false),
                        SplitEnd = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.LineID)
                .ForeignKey("Punch", t => t.PunchID, cascadeDelete: true)
                .ForeignKey("Timecard", t => t.TimecardID, cascadeDelete: true)
                .ForeignKey("PayType", t => t.PayTypeID, cascadeDelete: true)
                .Index(t => t.PunchID)
                .Index(t => t.TimecardID)
                .Index(t => t.PayTypeID);
            
            CreateTable(
                "MessageViewed",
                c => new
                    {
                        EmployeeID = c.Int(nullable: false),
                        MessageID = c.Int(nullable: false),
                        DateViewed = c.DateTime(nullable: false),
                        Employee_EmployeeID = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => new { t.EmployeeID, t.MessageID })
                .ForeignKey("Employee", t => t.Employee_EmployeeID)
                .Index(t => t.Employee_EmployeeID);
            
            CreateTable(
                "Timecard",
                c => new
                    {
                        TimecardID = c.Int(nullable: false, identity: true),
                        EmployeeID = c.Int(nullable: false),
                        PayPeriod = c.DateTime(nullable: false),
                        Employee_EmployeeID = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.TimecardID)
                .ForeignKey("Employee", t => t.Employee_EmployeeID)
                .Index(t => t.Employee_EmployeeID);
            
            CreateTable(
                "Message",
                c => new
                    {
                        MessageID = c.Int(nullable: false, identity: true),
                        Body = c.String(),
                        ManagerID = c.String(),
                        Manager_EmployeeID = c.String(maxLength: 128),
                        Employee_EmployeeID = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.MessageID)
                .ForeignKey("Employee", t => t.Manager_EmployeeID)
                .ForeignKey("Employee", t => t.Employee_EmployeeID)
                .Index(t => t.Manager_EmployeeID)
                .Index(t => t.Employee_EmployeeID);
            
            CreateTable(
                "Company",
                c => new
                    {
                        CompanyID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.CompanyID);
            
            CreateTable(
                "Department",
                c => new
                    {
                        DepartmentID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Location = c.String(),
                        PayPeriodSeed = c.DateTime(nullable: false),
                        PayPeriodInterval = c.Int(nullable: false),
                        Company_CompanyID = c.Int(),
                    })
                .PrimaryKey(t => t.DepartmentID)
                .ForeignKey("Company", t => t.Company_CompanyID)
                .Index(t => t.Company_CompanyID);
            
            CreateTable(
                "Holiday",
                c => new
                    {
                        HolidayID = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        Repeats = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.HolidayID);
            
            CreateTable(
                "PayType",
                c => new
                    {
                        PayTypeID = c.Int(nullable: false, identity: true),
                        DailyMax = c.Int(nullable: false),
                        WeeklyMax = c.Int(nullable: false),
                        NextPayType_PayTypeID = c.Int(),
                    })
                .PrimaryKey(t => t.PayTypeID)
                .ForeignKey("PayType", t => t.NextPayType_PayTypeID)
                .Index(t => t.NextPayType_PayTypeID);
            
            CreateTable(
                "PunchType",
                c => new
                    {
                        PunchTypeID = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        PunchInOption = c.String(),
                    })
                .PrimaryKey(t => t.PunchTypeID);
            
            CreateTable(
                "HolidayDepartment",
                c => new
                    {
                        Holiday_HolidayID = c.Int(nullable: false),
                        Department_DepartmentID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Holiday_HolidayID, t.Department_DepartmentID })
                .ForeignKey("Holiday", t => t.Holiday_HolidayID, cascadeDelete: true)
                .ForeignKey("Department", t => t.Department_DepartmentID, cascadeDelete: true)
                .Index(t => t.Holiday_HolidayID)
                .Index(t => t.Department_DepartmentID);
            
        }
        
        public override void Down()
        {
            DropIndex("HolidayDepartment", new[] { "Department_DepartmentID" });
            DropIndex("HolidayDepartment", new[] { "Holiday_HolidayID" });
            DropIndex("PayType", new[] { "NextPayType_PayTypeID" });
            DropIndex("Department", new[] { "Company_CompanyID" });
            DropIndex("Message", new[] { "Employee_EmployeeID" });
            DropIndex("Message", new[] { "Manager_EmployeeID" });
            DropIndex("Timecard", new[] { "Employee_EmployeeID" });
            DropIndex("MessageViewed", new[] { "Employee_EmployeeID" });
            DropIndex("Line", new[] { "PayTypeID" });
            DropIndex("Line", new[] { "TimecardID" });
            DropIndex("Line", new[] { "PunchID" });
            DropIndex("Punch", new[] { "PunchTypeID" });
            DropIndex("Punch", new[] { "DepartmentID" });
            DropIndex("Punch", new[] { "EmployeeID" });
            DropIndex("Employee", new[] { "DepartmentID" });
            DropIndex("Employee", new[] { "Message_MessageID" });
            DropIndex("Employee", new[] { "Manager_EmployeeID" });
            DropForeignKey("HolidayDepartment", "Department_DepartmentID", "Department");
            DropForeignKey("HolidayDepartment", "Holiday_HolidayID", "Holiday");
            DropForeignKey("PayType", "NextPayType_PayTypeID", "PayType");
            DropForeignKey("Department", "Company_CompanyID", "Company");
            DropForeignKey("Message", "Employee_EmployeeID", "Employee");
            DropForeignKey("Message", "Manager_EmployeeID", "Employee");
            DropForeignKey("Timecard", "Employee_EmployeeID", "Employee");
            DropForeignKey("MessageViewed", "Employee_EmployeeID", "Employee");
            DropForeignKey("Line", "PayTypeID", "PayType");
            DropForeignKey("Line", "TimecardID", "Timecard");
            DropForeignKey("Line", "PunchID", "Punch");
            DropForeignKey("Punch", "PunchTypeID", "PunchType");
            DropForeignKey("Punch", "DepartmentID", "Department");
            DropForeignKey("Punch", "EmployeeID", "Employee");
            DropForeignKey("Employee", "DepartmentID", "Department");
            DropForeignKey("Employee", "Message_MessageID", "Message");
            DropForeignKey("Employee", "Manager_EmployeeID", "Employee");
            DropTable("HolidayDepartment");
            DropTable("PunchType");
            DropTable("PayType");
            DropTable("Holiday");
            DropTable("Department");
            DropTable("Company");
            DropTable("Message");
            DropTable("Timecard");
            DropTable("MessageViewed");
            DropTable("Line");
            DropTable("Punch");
            DropTable("Employee");
        }
    }
}
