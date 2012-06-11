namespace TimeClock.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class undoDepartmentIntervalTypeChange : DbMigration
    {
        public override void Up()
        {
            AlterColumn("Department", "PayPeriodInterval", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("Department", "PayPeriodInterval", c => c.Time(nullable: false));
        }
    }
}
