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
                        ManagerID = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.EmployeeID)
                .ForeignKey("Employee", t => t.ManagerID)
                .ForeignKey("Department", t => t.DepartmentID, cascadeDelete: true)
                .Index(t => t.ManagerID)
                .Index(t => t.DepartmentID);
            
            CreateTable(
                "Punch",
                c => new
                    {
                        PunchID = c.Int(nullable: false, identity: true),
                        InTime = c.DateTime(nullable: false),
                        OutTime = c.DateTime(),
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
                        SplitStart = c.DateTime(nullable: false),
                        SplitEnd = c.DateTime(nullable: false),
                        PayTypeID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.LineID)
                .ForeignKey("PayType", t => t.PayTypeID, cascadeDelete: true)
                .ForeignKey("Punch", t => t.PunchID, cascadeDelete: true)
                .ForeignKey("Timecard", t => t.TimecardID, cascadeDelete: true)
                .Index(t => t.PayTypeID)
                .Index(t => t.PunchID)
                .Index(t => t.TimecardID);
            
            CreateTable(
                "PayType",
                c => new
                    {
                        PayTypeID = c.Int(nullable: false, identity: true),
                        DailyMax = c.Int(nullable: false),
                        WeeklyMax = c.Int(nullable: false),
                        Description = c.String(),
                        NextPayType_PayTypeID = c.Int(),
                        Department_DepartmentID = c.Int(),
                    })
                .PrimaryKey(t => t.PayTypeID)
                .ForeignKey("PayType", t => t.NextPayType_PayTypeID)
                .ForeignKey("Department", t => t.Department_DepartmentID)
                .Index(t => t.NextPayType_PayTypeID)
                .Index(t => t.Department_DepartmentID);
            
            CreateTable(
                "MessageViewed",
                c => new
                    {
                        EmployeeID = c.Int(nullable: false),
                        MessageID = c.Int(nullable: false),
                        DateViewed = c.DateTime(),
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
                        EmployeeID = c.String(maxLength: 128),
                        PayPeriod = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.TimecardID)
                .ForeignKey("Employee", t => t.EmployeeID)
                .Index(t => t.EmployeeID);
            
            CreateTable(
                "Message",
                c => new
                    {
                        MessageID = c.Int(nullable: false, identity: true),
                        Body = c.String(),
                        ManagerID = c.String(),
                        Manager_EmployeeID = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.MessageID)
                .ForeignKey("Employee", t => t.Manager_EmployeeID)
                .Index(t => t.Manager_EmployeeID);
            
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
                        Description = c.String(),
                        Date = c.DateTime(nullable: false),
                        Repeats = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.HolidayID);
            
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
                "EmployeeMessages",
                c => new
                    {
                        EmployeeID = c.String(nullable: false, maxLength: 128),
                        MessageID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.EmployeeID, t.MessageID })
                .ForeignKey("Employee", t => t.EmployeeID, cascadeDelete: true)
                .ForeignKey("Message", t => t.MessageID, cascadeDelete: true)
                .Index(t => t.EmployeeID)
                .Index(t => t.MessageID);
            
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
            DropIndex("EmployeeMessages", new[] { "MessageID" });
            DropIndex("EmployeeMessages", new[] { "EmployeeID" });
            DropIndex("Department", new[] { "Company_CompanyID" });
            DropIndex("Message", new[] { "Manager_EmployeeID" });
            DropIndex("Timecard", new[] { "EmployeeID" });
            DropIndex("MessageViewed", new[] { "Employee_EmployeeID" });
            DropIndex("PayType", new[] { "Department_DepartmentID" });
            DropIndex("PayType", new[] { "NextPayType_PayTypeID" });
            DropIndex("Line", new[] { "TimecardID" });
            DropIndex("Line", new[] { "PunchID" });
            DropIndex("Line", new[] { "PayTypeID" });
            DropIndex("Punch", new[] { "PunchTypeID" });
            DropIndex("Punch", new[] { "DepartmentID" });
            DropIndex("Punch", new[] { "EmployeeID" });
            DropIndex("Employee", new[] { "DepartmentID" });
            DropIndex("Employee", new[] { "ManagerID" });
            DropForeignKey("HolidayDepartment", "Department_DepartmentID", "Department");
            DropForeignKey("HolidayDepartment", "Holiday_HolidayID", "Holiday");
            DropForeignKey("EmployeeMessages", "MessageID", "Message");
            DropForeignKey("EmployeeMessages", "EmployeeID", "Employee");
            DropForeignKey("Department", "Company_CompanyID", "Company");
            DropForeignKey("Message", "Manager_EmployeeID", "Employee");
            DropForeignKey("Timecard", "EmployeeID", "Employee");
            DropForeignKey("MessageViewed", "Employee_EmployeeID", "Employee");
            DropForeignKey("PayType", "Department_DepartmentID", "Department");
            DropForeignKey("PayType", "NextPayType_PayTypeID", "PayType");
            DropForeignKey("Line", "TimecardID", "Timecard");
            DropForeignKey("Line", "PunchID", "Punch");
            DropForeignKey("Line", "PayTypeID", "PayType");
            DropForeignKey("Punch", "PunchTypeID", "PunchType");
            DropForeignKey("Punch", "DepartmentID", "Department");
            DropForeignKey("Punch", "EmployeeID", "Employee");
            DropForeignKey("Employee", "DepartmentID", "Department");
            DropForeignKey("Employee", "ManagerID", "Employee");
            DropTable("HolidayDepartment");
            DropTable("EmployeeMessages");
            DropTable("PunchType");
            DropTable("Holiday");
            DropTable("Department");
            DropTable("Company");
            DropTable("Message");
            DropTable("Timecard");
            DropTable("MessageViewed");
            DropTable("PayType");
            DropTable("Line");
            DropTable("Punch");
            DropTable("Employee");
        }
    }
}
