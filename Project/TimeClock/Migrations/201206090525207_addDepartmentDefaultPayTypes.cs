namespace TimeClock.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class addDepartmentDefaultPayTypes : DbMigration
    {
        public override void Up()
        {
            AddColumn("PayType", "SeventhDayMax", c => c.Int(nullable: false));
            AddColumn("Department", "DefaultPayType_PayTypeID", c => c.Int());
            AddColumn("Department", "DefaultSickType_PayTypeID", c => c.Int());
            AddColumn("Department", "DefaultHolidayType_PayTypeID", c => c.Int());
            AddForeignKey("Department", "DefaultPayType_PayTypeID", "PayType", "PayTypeID");
            AddForeignKey("Department", "DefaultSickType_PayTypeID", "PayType", "PayTypeID");
            AddForeignKey("Department", "DefaultHolidayType_PayTypeID", "PayType", "PayTypeID");
            CreateIndex("Department", "DefaultPayType_PayTypeID");
            CreateIndex("Department", "DefaultSickType_PayTypeID");
            CreateIndex("Department", "DefaultHolidayType_PayTypeID");
        }
        
        public override void Down()
        {
            DropIndex("Department", new[] { "DefaultHolidayType_PayTypeID" });
            DropIndex("Department", new[] { "DefaultSickType_PayTypeID" });
            DropIndex("Department", new[] { "DefaultPayType_PayTypeID" });
            DropForeignKey("Department", "DefaultHolidayType_PayTypeID", "PayType");
            DropForeignKey("Department", "DefaultSickType_PayTypeID", "PayType");
            DropForeignKey("Department", "DefaultPayType_PayTypeID", "PayType");
            DropColumn("Department", "DefaultHolidayType_PayTypeID");
            DropColumn("Department", "DefaultSickType_PayTypeID");
            DropColumn("Department", "DefaultPayType_PayTypeID");
            DropColumn("PayType", "SeventhDayMax");
        }
    }
}
