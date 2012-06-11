using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;
using TimeClock.Resources;

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
        [ForeignKey("DepartmentID")]
        public virtual Department department { get; set; }

        /* One to Many */
        public virtual ICollection<Punch> Punches { get; set; }
        [ForeignKey("EmployeeID")]
        public virtual ICollection<MessageViewed> ViewedMessages { get; set; }
        [ForeignKey("EmployeeID")]
        public virtual ICollection<Timecard> Timecards { get; set; }
        
        /* Many to Many */
        public virtual ICollection<Message> Messages { get; set; }

        public bool workedSixPreviousDays(TimeClockContext db)
        {
            return consecutiveDaysWorked(db) >= 6;
        }


        public int consecutiveDaysWorked(TimeClockContext db)
        {

            return 0;
        }

        private int consecutiveDaysWorkedHelper(TimeClockContext db, int days)
        {


            return 0;
        }

        public double minutsWorkedDate(TimeClockContext db, DateTime date)
        {
            PayPeriod currentPP = PayPeriodTools.LookupPayPeriod(db, DepartmentID, date);
            Timecard tc = db.Timecards.SingleOrDefault(t => t.PayPeriod.Equals(currentPP.Start) && t.EmployeeID.Equals(EmployeeID));

            //Get all lines for this timecard
            var allLines = db.Lines.Where(l => l.TimecardID == tc.TimecardID).ToList();

            //Need to remove the lines that don't match our logic, LINQ wasn't a fan of this so just pull them out one at a time.
            List<Line> lines = new List<Line>();
            foreach (Line line in allLines)
            {
                if (line.SplitStart.Subtract(date).TotalMinutes >= 0 && line.SplitEnd.Subtract(date.AddDays(1)).TotalMinutes <= 0)
                    lines.Add(line);
            }

            double minutesWorked = 0;

            foreach (Line line in lines)
                minutesWorked += line.getDuration().TotalMinutes;

            return minutesWorked;
        }

        public double minutesWorkedWeek(TimeClockContext db, DateTime date)
        {

            // Determine the Date of the start of the current week
            DateTime seed = department.PayPeriodSeed;
            int interval = 7; // One week, so we can find the current start

            TimeSpan duration = date.Subtract(seed);

            int count = (int)Math.Floor(duration.TotalDays / (double)interval);

            DateTime weekStart = seed.Add(TimeSpan.FromDays(interval * count));
            date = weekStart;

            double minutesWorked = 0;

            for (int i = 0; i <= interval; i++)
            {
                minutesWorked += minutsWorkedDate(db, date);
                date.AddDays(1);
            }

            return minutesWorked;

            /*
            //Get the current time card and the next
            var timecards = db.Timecards.Where(tc => tc.EmployeeID == EmployeeID && tc.PayPeriod.Equals(payPeriod.Start)).ToList(); 
            
            // If we are ahead in time, add the next timecard if it exist
            if (date.Subtract(payPeriod.End).TotalDays >= 0)
                timecards.Concat( db.Timecards.Where(tc => tc.EmployeeID == EmployeeID && tc.PayPeriod.Equals(payPeriod.End)));
            
            //Get all lines for this timecard
            var curTimecardID = timecards.ElementAt(0).TimecardID;
            var lines = db.Lines.Where(l => l.TimecardID == curTimecardID).ToList();

            // If a second timecard was found add the lines from it aswell
            if (timecards.Count() > 1)
            {
                var nextTimecardID = timecards.ElementAt(1).TimecardID;
                lines.Concat(db.Lines.Where(l => l.TimecardID == nextTimecardID));
            }

            //Need to remove the lines that don't match our logic, LINQ wasn't a fan of this so just pull them out one at a time.
            foreach (Line line in lines)
            {
                if (!(line.SplitStart.Subtract(weekStart).TotalMinutes >= 0 && line.SplitEnd.Subtract(weekStart.AddDays(7)).TotalMinutes <= 0))
                    lines.Remove(line);
            }
/*
            // Count the minutes worked during the following week.
            double minutesWorked = 0;

            foreach (Line line in lines)
                minutesWorked += line.getDuration().TotalMinutes;
            
            return minutesWorked; */
        }


        public int isWorking(TimeClockContext db)
        {
            PayPeriod payPeriod = PayPeriodTools.LookupPayPeriod(db, 1);

            var empPunches = db.Punches.Where(p => p.EmployeeID == EmployeeID && p.InTime > payPeriod.Start && p.InTime < payPeriod.End && p.OutTime == null);
                       
            if (empPunches.Count(p => p.OutTime == null) != 0)
            {
                var lastPunch = empPunches.OrderBy(p => p.InTime).AsEnumerable().Last();
           
                if (DateTime.Now.Subtract(lastPunch.InTime) < TimeSpan.FromHours(24))
                    return lastPunch.PunchID;
            }
            return -1;
        }

        public IEnumerable<Message> PendingMessages(TimeClockContext db)
        {
            List<Message> myMessages = Messages.ToList();

            foreach (Message m in myMessages)
            {
                var isViewed = db.MessagesViewed.SingleOrDefault(v => v.MessageID == m.MessageID && v.EmployeeID == this.EmployeeID);
                if (isViewed != null) myMessages.Remove(m);
            }

            return myMessages.AsEnumerable();
        }

        public bool rebuildTimecardLines(TimeClockContext db, Timecard tc)
        {
            var lines = db.Lines.Where(l => l.TimecardID == tc.TimecardID);

            var punches = db.Punches.Where(p => lines.Any(l => l.PunchID == p.PunchID)).ToList();

            var lineIDs = from l in db.Lines
                    where punches.Contains(l.Punch)
                    select l.LineID;
            List<int> lineList = lineIDs.ToList<int>();
            
            if (lineList != null)
            {
                //List<int> lineIDs = lines.Select(l => l.LineID).ToList<int>();
                foreach (int lineID in lineList) {
                    var line = db.Lines.SingleOrDefault(l => l.LineID == lineID);
                    db.Lines.Remove(line);
                }
                db.SaveChanges();
            }

            foreach (Punch punch in punches)
                Calculations.addLines(db, punch);

           /*
            List<Punch> punches = new List<Punch>();

            foreach (Line line in lines)
            {
                // Add the punch to a list of punches that needs to be redone
                if (! punches.Contains(line.Punch))
                    punches.Add(line.Punch);

                // Remove the line from the db
                db.Lines.Remove(line);
            }

            db.SaveChanges(); // All the lines for this payperiod is removed

            // Add all the lines for all the punches removed.
            foreach (Punch punch in punches)
                Calculations.addLines(db, punch);
            */
            return true;
        }

        public IEnumerable<TimeCardView> getTimeCardLines(TimeClockContext db, PayPeriod payPeriod)
        {
            int lineNumberCounter = 1;
            List<TimeCardView> timecard = new List<TimeCardView>();
            
            var empTC = db.Timecards.SingleOrDefault(t => t.EmployeeID == this.EmployeeID && t.PayPeriod == payPeriod.Start);

            //Need to bail out if we don't have time cards
            if (empTC == null) return timecard;

            var lines = db.Lines.Where(l => l.TimecardID == empTC.TimecardID);
            
            //Need to bail out if we don't have time card lines
            if (lines == null) return timecard;
            
            lines.OrderBy(l => l.SplitStart);

            foreach (Line line in lines)
            {
                int last = timecard.Count - 1;
                if (last > 0 && timecard[last].PunchID == line.PunchID)
                {
                    timecard[last].Out = line.SplitEnd;
                    if (line.PayType.Description == "Overtime")
                        timecard[last].Overtime = line.SplitEnd.Subtract(line.SplitStart).TotalHours;
                    else if (line.PayType.Description == "Regular")
                        timecard[last].Regular = line.SplitEnd.Subtract(line.SplitStart).TotalHours;
                    //else ;

                    timecard[last].updateEntry();
                }

                // Otherwise we create a new line and add it to the timecard.
                else
                {
                    TimeCardView temp = new TimeCardView(lineNumberCounter, line.SplitStart.Date, line.SplitStart, line.SplitEnd, line.PunchID);
                    if (line.PayType.Description == "Regular")
                        temp.Regular = line.SplitStart.Subtract(line.SplitEnd).TotalHours;
                    else if (line.PayType.Description == "Overtime")
                        temp.Overtime = line.SplitStart.Subtract(line.SplitEnd).TotalHours;
                    //else ;// What should we do if it is neither of the two?

                    timecard.Add(temp);
                }
            }

            return timecard;
        }

    }

    public class Company
    {
        public int CompanyID { get; set; }
        public string Name { get; set; }
    }

    public class Department
    {
        public int          DepartmentID        { get; set; }
        public string       Name                { get; set; }
        public string       Location            { get; set; }
        
        public DateTime     PayPeriodSeed       { get; set; } //First day of the first payperiod.
        public int          PayPeriodInterval   { get; set; }  //Number of days in a pay period.
        public virtual Company Company          { get; set; }

        /* One to One */
        public virtual PayType DefaultPayType       { get; set; }
        public virtual PayType DefaultSickType      { get; set; }
        public virtual PayType DefaultHolidayType   { get; set; }

        /* One to Many */
        public virtual ICollection<Employee>    Employees   { get; set; }
        public virtual ICollection<Punch>       Punches     { get; set; }

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
        public string EmployeeID { get; set; }
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
        public int      PayTypeID       { get; set; }
        public int      DailyMax        { get; set; }
        public int      WeeklyMax       { get; set; }
        public int      SeventhDayMax   { get; set; }
        public string   Description     { get; set; }
        
        /* One to One */
        public virtual PayType NextPayType { get; set; }

        public int getDailyMax(bool seventhDay)
        {
            if (seventhDay)
                return SeventhDayMax * 60;
            else
                return DailyMax * 60;
        }

        public int getWeeklyMax(bool seventhDay)
        {
            return WeeklyMax * 60;
        }

    }

    public class Punch
    {
        public int PunchID { get; set; }
        public DateTime InTime { get; set; }
        public DateTime? OutTime { get; set; }
        public int DepartmentID { get; set; }
        public string EmployeeID { get; set; }
        public int PunchTypeID { get; set; }
    
        /*One to One */
        [ForeignKey("EmployeeID")]
        public virtual Employee employee { get; set; }
        
        /* One to Many */
        public virtual ICollection<Line> Lines { get; set; }

        public TimeSpan getDuration()
        {
            if(OutTime.HasValue)
                return OutTime.Value.Subtract(InTime);
            else
                return new TimeSpan(0, 0, 0);
        }


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
        [ForeignKey("PunchID")]
        public virtual Punch Punch { get; set; }

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

        public DateTime getStartingDay(TimeClockContext db)
        {
            Employee emp = db.Employees.SingleOrDefault(e => e.EmployeeID.Equals(EmployeeID));

            DateTime seed = emp.department.PayPeriodSeed;
            int len = emp.department.PayPeriodInterval;

            TimeSpan duration;

            if (len > 0) duration = TimeSpan.FromDays(len);
            else duration = TimeSpan.MaxValue;
           
            while ( DateTime.Now.Subtract(seed).TotalMinutes > 0)
               seed.Add(duration);

            seed.Subtract(duration);

            return seed;
            
        }

    }

}