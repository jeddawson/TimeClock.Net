namespace TimeClock.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class modLineAndTimecard : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("PayTypeDepartment", "PayType_PayTypeID", "PayType");
            DropForeignKey("PayTypeDepartment", "Department_DepartmentID", "Department");
            DropIndex("PayTypeDepartment", new[] { "PayType_PayTypeID" });
            DropIndex("PayTypeDepartment", new[] { "Department_DepartmentID" });
            RenameColumn(table: "Timecard", name: "Employee_EmployeeID", newName: "Employee");
            CreateTable(
                "DepartmentPayType",
                c => new
                    {
                        Department_DepartmentID = c.Int(nullable: false),
                        PayType_PayTypeID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Department_DepartmentID, t.PayType_PayTypeID })
                .ForeignKey("Department", t => t.Department_DepartmentID, cascadeDelete: true)
                .ForeignKey("PayType", t => t.PayType_PayTypeID, cascadeDelete: true)
                .Index(t => t.Department_DepartmentID)
                .Index(t => t.PayType_PayTypeID);
            
            AlterColumn("Timecard", "EmployeeID", c => c.String(maxLength: 128));
            DropTable("PayTypeDepartment");
        }
        
        public override void Down()
        {
            CreateTable(
                "PayTypeDepartment",
                c => new
                    {
                        PayType_PayTypeID = c.Int(nullable: false),
                        Department_DepartmentID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.PayType_PayTypeID, t.Department_DepartmentID });
            
            DropIndex("DepartmentPayType", new[] { "PayType_PayTypeID" });
            DropIndex("DepartmentPayType", new[] { "Department_DepartmentID" });
            DropForeignKey("DepartmentPayType", "PayType_PayTypeID", "PayType");
            DropForeignKey("DepartmentPayType", "Department_DepartmentID", "Department");
            AlterColumn("Timecard", "EmployeeID", c => c.Int(nullable: false));
            DropTable("DepartmentPayType");
            RenameColumn(table: "Timecard", name: "EmployeeID", newName: "Employee_EmployeeID");
            CreateIndex("PayTypeDepartment", "Department_DepartmentID");
            CreateIndex("PayTypeDepartment", "PayType_PayTypeID");
            AddForeignKey("PayTypeDepartment", "Department_DepartmentID", "Department", "DepartmentID", cascadeDelete: true);
            AddForeignKey("PayTypeDepartment", "PayType_PayTypeID", "PayType", "PayTypeID", cascadeDelete: true);
        }
    }
}
