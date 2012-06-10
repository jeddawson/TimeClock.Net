using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TimeClock.Models;


/**
 *  Serializable Helper Objects
 *  
 *  The objects in this file will be used to serialize the inbound and outbound data.
 * 
**/

namespace TimeClock.Resources
{
       
    public class EmployeeStatus
    {
        public string   EmployeeID      { get; set; }
        public string   punchDirection  { get; set; }
        public int      openPunch       { get; set; }
    }


    /**
     *  Inbound Request - Punch Request Definition
     *  
     *  Serializable class that allows JSON/XML objects to be serialized in the HTTP POST body.
     *  Object should look like: [{ Id: "", Pin: "", closesPunch: -1, Timestamp: "", HMAC: "" }]
     * 
     *  Any field can be ommited and the object will still serialize, however the relying code
     *  may not perform as expected.
     * 
    **/

    public class PunchRequest
    {
        public string   ID              { get; set; }
        public string   pin             { get; set; }
        public int      closesPunch     { get; set; }
        public int      departmentID    { get; set; }
        public string   timestamp       { get; set; }
        public string   HMAC            { get; set; }
    }

    public class PunchResponse
    {
        public bool     isSuccess       { get; set; }
        public string   pinError        { get; set; }
        public string   generalError    { get; set; }
        public IEnumerable<TimeCardView> timecardData { get; set; }
    }

    public class ClockInitialItem
    {
        public string   employeeName    { get; set; }
        public string   employeeID      { get; set; }
        public int      departmentID    { get; set; }
    }

    public class MessageRead
    {
        public int messageID { get; set; }
        public string employeeID { get; set; }
        public string timestamp { get; set; }
        public string HMAC { get; set; }
    }

    public class MessageData
    {
        public int      ID              { get; set; }
        public string   from            { get; set; }
        public string   subject         { get; set; }
        public string   body            { get; set; }
        public bool     isNew           { get; set; }
    }

    public class Calculations
    {
        public static bool addLines(TimeClockContext db, Punch punch) 
        {
            // determine payperiod
            PayPeriod currentPayP = PayPeriodTools.LookupPayPeriod(db, punch.employee.department.DepartmentID, punch.OutTime.Value);
            Timecard currentTC = db.Timecards.SingleOrDefault(tc => tc.EmployeeID == punch.EmployeeID && tc.PayPeriod.Equals(currentPayP.Start));

            // check if we reach over payperiods
            if (punch.InTime.Subtract(currentPayP.Start).TotalMinutes < 0) // We started in the previous payperiod , solit the punch into two.
            {
                PayPeriod previousPayP = new PayPeriod()
                    {
                        Start = currentPayP.Start.Subtract(TimeSpan.FromDays(punch.employee.department.PayPeriodInterval)),
                        End = currentPayP.Start
                    };

                Timecard previousTC = db.Timecards.SingleOrDefault(tc => tc.EmployeeID == punch.EmployeeID && tc.PayPeriod.Equals(previousPayP.Start));

                bool first = addLinesTimecard(db, punch, previousTC, punch.InTime, previousPayP.End);
                bool second = addLinesTimecard(db, punch, currentTC, currentPayP.Start, punch.OutTime.Value);

                return first && second;
            }
            else if (punch.OutTime.Value.Subtract(currentPayP.End).TotalMinutes > 0) // We ended in the next payperiod - Not sure this will ever happen
            {
                PayPeriod nextPayP = new PayPeriod()
                    {
                        Start = currentPayP.End,
                        End = currentPayP.End.Add(TimeSpan.FromDays(punch.employee.department.PayPeriodInterval))
                    };
                Timecard nextTC = db.Timecards.SingleOrDefault(tc => tc.EmployeeID == punch.EmployeeID && tc.PayPeriod.Equals(nextPayP.Start));

                bool first = addLinesTimecard(db, punch, currentTC, punch.InTime, currentPayP.End);
                bool second = addLinesTimecard(db, punch, nextTC, nextPayP.Start, punch.OutTime.Value);

                return first && second;
            }
            else // No over lap, we just add the whole punch to the current Time card
            {
                return addLinesTimecard(db, punch, currentTC, punch.InTime, punch.OutTime.Value);
            }
            
        }


        /* This functions determine the weekly minuts worked, as well as the time that the current payday started.
         * 
         * This information is then used to call addLinesDaily(...);
         */
 
        private static bool addLinesTimecard(TimeClockContext db, Punch punch, Timecard tc, DateTime splitStart, DateTime splitEnd)
        {
            // Calculate weeklyWorked, and dailyWorked
            var lines = db.Lines.Where(l => l.TimecardID == tc.TimecardID);

           double weeklyMinuts = punch.employee.minutesWorkedWeek(db, splitStart); 

           TimeSpan dayBeginTime = punch.employee.department.PayPeriodSeed.TimeOfDay;
           DateTime currentDayStart = DateTime.Now.Date.Add(dayBeginTime);
            
            return addLinesDaily(db, punch, tc, splitStart, splitEnd, weeklyMinuts, currentDayStart);
        }

        /*
         * this split up the punch to the different days that it happen, this is done recursively, and call allLinesHelper on the corresponding days
         * 
         */
        private static bool addLinesDaily(TimeClockContext db, Punch punch, Timecard tc, DateTime splitStart, DateTime splitEnd, double weeklyWorked, DateTime dayStartTime)
        {
            // Split on days
            if(splitStart.Subtract(dayStartTime).TotalMinutes < 0) // the punch started on the previous day, split and call recursively
            {
                addLinesDaily(db, punch, tc, splitStart, dayStartTime, weeklyWorked, dayStartTime.AddDays(-1));
                splitStart = dayStartTime; // To continue adding the rest of the punch
            }


            if(splitEnd.Subtract(dayStartTime.AddDays(1)).TotalMinutes < 0) // the punch ended today, we can safely add it
            {
                double dailyworked = punch.employee.minutsWorkedDate(db, tc, dayStartTime);
                bool seventhDay = punch.employee.workedSixPreviousDays(db);
                addLinesHelper(db, punch, tc, punch.employee.department.DefaultPayType, weeklyWorked, dailyworked, splitStart, splitEnd, seventhDay);
            }
            else // The punch ends on the next day
            {
                double dailyworked = punch.employee.minutsWorkedDate(db, tc, dayStartTime);
                bool seventhDay = punch.employee.workedSixPreviousDays(db);
                addLinesHelper(db, punch, tc, punch.employee.department.DefaultPayType, weeklyWorked, dailyworked, splitStart, dayStartTime.AddDays(1), seventhDay);

                addLinesDaily(db, punch, tc, dayStartTime.AddDays(1), splitEnd, weeklyWorked, dayStartTime.AddDays(1));
            }

            return true;
        }
        

       private static bool addLinesHelper(TimeClockContext db, Punch punch, Timecard tc, PayType pt, double weeklyWorked, double dailyWorked, DateTime splitStart, DateTime splitEnd, bool seventhDay)
        {
            // a simple base case, to stop spawing extra lines -- Not sure if this is needed
            if (splitStart.Subtract(splitEnd).TotalMinutes == 0)
                return true;

        // Determin the correct pay type for this line.
           while (weeklyWorked > pt.getWeeklyMax(seventhDay))
               pt = pt.NextPayType;

           while (dailyWorked > pt.getDailyMax(seventhDay))
               pt = pt.NextPayType;

        // Calculate dailyLeft and weeklyLeft
           double dailyLeft = (double) pt.getDailyMax(seventhDay) - dailyWorked;
           double weeklyLeft = (double) pt.getWeeklyMax(seventhDay) - dailyWorked;

           double splitLength = splitEnd.Subtract(splitStart).TotalMinutes;


        // Check if we reach over the weekly -- if
           if (weeklyWorked + splitLength > pt.getWeeklyMax(seventhDay))
           {
               addLinesHelper(db, punch, tc, pt, weeklyWorked, dailyWorked, splitStart, splitStart.AddMinutes(weeklyLeft), seventhDay);
               addLinesHelper(db, punch, tc, pt.NextPayType, weeklyWorked + weeklyLeft, dailyWorked + weeklyLeft, splitStart.AddMinutes(weeklyLeft), splitEnd, seventhDay);
           }
               // Check if we reached over the daily limit
           else if (dailyWorked + splitLength > pt.getDailyMax(seventhDay))
           {
               addLinesHelper(db, punch, tc, pt, weeklyWorked, dailyWorked, splitStart, splitStart.AddMinutes(dailyLeft), seventhDay);
               addLinesHelper(db, punch, tc, pt.NextPayType, weeklyWorked + dailyLeft, dailyWorked + dailyLeft, splitStart.AddMinutes(dailyLeft), splitEnd, seventhDay);
           }
               // we can safely add the line to the DB
           else 
           {
               db.Lines.Add(new Line()
                    {
                        TimecardID = tc.TimecardID,
                        PunchID = punch.PunchID,
                        SplitStart = splitStart,
                        SplitEnd = splitEnd,
                        PayTypeID = pt.PayTypeID
                    });
               db.SaveChanges();
             
           }

           return true;
       }

    }
}
