// <auto-generated />
namespace TimeClock.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Infrastructure;
    
    public sealed partial class addDepartmentDefaultPayTypes : IMigrationMetadata
    {
        string IMigrationMetadata.Id
        {
            get { return "201206090525207_addDepartmentDefaultPayTypes"; }
        }
        
        string IMigrationMetadata.Source
        {
            get { return null; }
        }
        
        string IMigrationMetadata.Target
        {
            get { return "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"; }
        }
    }
}