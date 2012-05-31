using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace TimeClock.Models
{
    public class TimeClockContext : DbContext
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Holiday> Holidays { get; set; }
        public DbSet<PayType> PayTypes { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageViewed> MessagesViewed { get; set; } 
        public DbSet<Punch> Punches { get; set; }
        public DbSet<PunchType> PunchTypes { get; set; }
        public DbSet<Timecard> Timecards { get; set; }
        public DbSet<Line> Lines { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Entity<MessageViewed>().HasKey(k => new { k.EmployeeID, k.MessageID });
            modelBuilder.Entity<Employee>()
                .HasMany(e => e.Messages)
                .WithMany(m => m.Employees)
                .Map(x =>
                    {
                        x.MapLeftKey("EmployeeID");
                        x.MapRightKey("MessageID");
                        x.ToTable("EmployeeMessages");
                    });

        }
    }

    public class TimeClockInitializer : DropCreateDatabaseIfModelChanges<TimeClockContext>
    {
        protected void Seed(TimeClockContext context)
        {
            base.Seed(context);
        }
    }

    public class Employee
    {
        public string EmployeeID { get; set; }
        public string FirstName { get; set; }
        public char? MiddleInitial { get; set; }
        public string LastName { get; set; }
        public bool Terminated { get; set; }
        public string Pin { get; set; }
        public int DepartmentID { get; set; }
        
        /* One to One */
        public string ManagerID { get; set; }
        [ForeignKey("ManagerID")]
        public virtual Employee Manager { get; set;}
        
        /* One to Many */
        public virtual ICollection<Punch> Punches { get; set; }
        public virtual ICollection<MessageViewed> ViewedMessages { get; set; }
        [ForeignKey("EmployeeID")]
        public virtual ICollection<Timecard> Timecards { get; set; }

        /* Many to Many */
        public virtual ICollection<Message> Messages { get; set; }
    }

    public class Company
    {
        public int CompanyID { get; set; }
        public string Name { get; set; }
    }

    public class Department
    {
        public int DepartmentID { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        
        public DateTime PayPeriodSeed { get; set; } //First day of the first payperiod.
        public int PayPeriodInterval { get; set; }  //Number of days in a pay period.

        public virtual Company Company { get; set; }
        
        /* One to Many */
        public virtual ICollection<Employee> Employees { get; set; }
        public virtual ICollection<Punch> Punches { get; set; }

        /* Many to Many */
        public virtual ICollection<Holiday> Holidays { get; set; }
        public virtual ICollection<PayType> PayTypes { get; set; }
    }

    public class Message
    {
        public int MessageID { get; set; }
        public string Body { get; set; }
        
        /* One to One */
        public string ManagerID { get; set; }
        public virtual Employee Manager { get; set; }

        /* Many to Many */
        public virtual ICollection<Employee> Employees { get; set; }
    }

    public class MessageViewed
    {
       // [Key, Column(Order = 0)]
        public int MessageID { get; set; }
       // [Key, Column(Order=1)]
        public int EmployeeID { get; set; }
        public DateTime? DateViewed { get; set; }
    }

    public class Holiday
    {
        public int HolidayID { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public int Repeats { get; set; }

        /* Many to Many */
        public virtual ICollection<Department> Departments { get; set; }
    }

    public class PayType
    {
        public int PayTypeID { get; set; }
        public int DailyMax { get; set; }
        public int WeeklyMax { get; set; }
        public string Description { get; set; }
        
        /* One to One */
        public virtual PayType NextPayType { get; set; }
    }

    public class Punch
    {
        public int PunchID { get; set; }
        public DateTime InTime { get; set; }
        public DateTime? OutTime { get; set; }
        public int DepartmentID { get; set; }
        public string EmployeeID { get; set; }
        public int PunchTypeID { get; set; }

        /* One to Many */
        public virtual ICollection<Line> Lines { get; set; }
    }

    public class PunchType
    {
        public int PunchTypeID { get; set; }
        public string Description { get; set; }
        public string PunchInOption { get; set; }

        /* One to Many */
        public virtual ICollection<Punch> Punches { get; set; }
    }

    public class Line
    {
        public int LineID { get; set; }
        public int PunchID { get; set; }
        public int TimecardID { get; set; }
        
        public DateTime SplitStart { get; set; }
        public DateTime SplitEnd { get; set; }
      
        public int PayTypeID { get; set; }
        [ForeignKey("PayTypeID")]
        public virtual PayType PayType { get; set; }

        public TimeSpan getDuration() 
        {
            return SplitEnd.Subtract(SplitStart);
        }

    }

    public class Timecard
    {
        public int TimecardID { get; set; }
        public string EmployeeID { get; set; }
        public DateTime PayPeriod { get; set; }

        /* One to Many */
        [ForeignKey("TimecardID")]
        public virtual ICollection<Line> Lines { get; set; }
    }

}