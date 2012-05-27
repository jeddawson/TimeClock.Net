namespace TimeClock.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class employeeIDResolution : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "Timecard", name: "Employee", newName: "EmployeeID");
        }
        
        public override void Down()
        {
            RenameColumn(table: "Timecard", name: "EmployeeID", newName: "Employee");
        }
    }
}
