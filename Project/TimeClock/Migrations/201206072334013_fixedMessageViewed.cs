namespace TimeClock.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class fixedMessageViewed : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("MessageViewed", "Employee_EmployeeID", "Employee");
            DropIndex("MessageViewed", new[] { "Employee_EmployeeID" });
            RenameColumn(table: "MessageViewed", name: "Employee_EmployeeID", newName: "EmployeeID");
            AlterColumn("MessageViewed", "EmployeeID", c => c.String(nullable: false, maxLength: 128));
            AddForeignKey("MessageViewed", "EmployeeID", "Employee", "EmployeeID", cascadeDelete: true);
            CreateIndex("MessageViewed", "EmployeeID");
        }
        
        public override void Down()
        {
            DropIndex("MessageViewed", new[] { "EmployeeID" });
            DropForeignKey("MessageViewed", "EmployeeID", "Employee");
            AlterColumn("MessageViewed", "EmployeeID", c => c.Int(nullable: false));
            RenameColumn(table: "MessageViewed", name: "EmployeeID", newName: "Employee_EmployeeID");
            CreateIndex("MessageViewed", "Employee_EmployeeID");
            AddForeignKey("MessageViewed", "Employee_EmployeeID", "Employee", "EmployeeID");
        }
    }
}
