namespace TimeClock.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class Latest : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "PayTypeDepartment",
                c => new
                    {
                        PayType_PayTypeID = c.Int(nullable: false),
                        Department_DepartmentID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.PayType_PayTypeID, t.Department_DepartmentID })
                .ForeignKey("PayType", t => t.PayType_PayTypeID, cascadeDelete: true)
                .ForeignKey("Department", t => t.Department_DepartmentID, cascadeDelete: true)
                .Index(t => t.PayType_PayTypeID)
                .Index(t => t.Department_DepartmentID);
            
            AddColumn("Holiday", "Description", c => c.String());
            AddColumn("PayType", "Description", c => c.String());
            AlterColumn("Punch", "OutTime", c => c.DateTime());
            AlterColumn("MessageViewed", "DateViewed", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropIndex("PayTypeDepartment", new[] { "Department_DepartmentID" });
            DropIndex("PayTypeDepartment", new[] { "PayType_PayTypeID" });
            DropForeignKey("PayTypeDepartment", "Department_DepartmentID", "Department");
            DropForeignKey("PayTypeDepartment", "PayType_PayTypeID", "PayType");
            AlterColumn("MessageViewed", "DateViewed", c => c.DateTime(nullable: false));
            AlterColumn("Punch", "OutTime", c => c.DateTime(nullable: false));
            DropColumn("PayType", "Description");
            DropColumn("Holiday", "Description");
            DropTable("PayTypeDepartment");
        }
    }
}
