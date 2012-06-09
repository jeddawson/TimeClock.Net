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
            PayPeriod payp = PayPeriodTools.LookupPayPeriod(db, );

            // determine payperiod
            // check if we reach over payperiods

                // if we do split accordingly and call addLinesTimecard

            // just call addLinesTimecard
            return true;

        }

        private static bool addLinesTimecard(TimeClockContext db, Punch punch, Timecard tc, DateTime splitStart, DateTime splitEnd)
        {
            // Calculate weeklyWorked, and dailyWorked

            // Split on days

                // Determine if special seventh day

            // call addLinesHelper with the default paytype for the department



            return true;
        }

       private static bool addLinesHelper(TimeClockContext db, Punch punch, Timecard tc, PayType pt, double weeklyWorked, double dailyWorked, DateTime splitStart, DateTime splitEnd, bool secenthDay)
        {
        // Determin the correct pay type for this line.

        // Calculate dailyLeft and weeklyLeft

        // Check if we reach over the weekly -- if

           // If we do add the first part, recurse on what is left

        // check if we are over daily max    -- else if

           // if we are add the first part, recurse on what is left

        // if we get here we can add with out any problems -- else


           return true;
       }

    }
    }


    
}