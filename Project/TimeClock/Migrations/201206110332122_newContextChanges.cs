namespace TimeClock.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class newContextChanges : DbMigration
    {
        public override void Up()
        {
            AddColumn("PayType", "ExportValue", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("PayType", "ExportValue");
        }
    }
}
