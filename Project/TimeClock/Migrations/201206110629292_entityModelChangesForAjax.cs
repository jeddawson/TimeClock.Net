namespace TimeClock.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class entityModelChangesForAjax : DbMigration
    {
        public override void Up()
        {
            AddColumn("Message", "DateCreated", c => c.DateTime(nullable: false));
            AddColumn("Message", "Subject", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("Message", "Subject");
            DropColumn("Message", "DateCreated");
        }
    }
}
