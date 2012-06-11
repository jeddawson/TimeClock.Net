using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using TimeClock.Models;

namespace TimeClock.Resources
{
    public class PayPeriodTools
    {
        public String getAllPayroll(TimeClockContext db, PayPeriod payp)
        {
            var employees = db.Employees;
            String retString = "";
            foreach (Employee emp in employees)
            {
                Timecard tc = db.Timecards.SingleOrDefault(time => time.PayPeriod.Equals(payp.Start) && time.EmployeeID.Equals(emp.EmployeeID));

                var lines = db.Lines.Where(l => l.TimecardID == tc.TimecardID);

                Dictionary<String, Double> lineInformation = new Dictionary<String, Double>();

                foreach (Line line in lines)
                {
                    string key = line.Punch.DepartmentID.ToString() + ", " + line.PayType.ExportValue;

                    if (lineInformation.ContainsKey(key))
                        lineInformation[key] += line.getMinutDuration();
                    else
                        lineInformation.Add(key, line.getMinutDuration());
                }

                foreach (String key in lineInformation.Keys)
                {
                    String payRollLine = emp.EmployeeID + ", " + key + ", " + lineInformation[key].ToString("F2");
                    retString += payRollLine + "\n";
                }
            }

            return retString;
        }


        /* just whipped this up... need to test the logic and make sure we get the first day of the current pay period! */
        public static PayPeriod LookupPayPeriod(TimeClockContext db, int DepartmentID)
        {
            return LookupPayPeriod(db, DepartmentID, DateTime.Now);

        }

        public static PayPeriod LookupPayPeriod(TimeClockContext db, int DepartmentID, DateTime time)
        {
            var seed = db.Departments.SingleOrDefault(d => d.DepartmentID == DepartmentID);

            DateTime seedDate = seed.PayPeriodSeed;
            int interval = seed.PayPeriodInterval;

            TimeSpan span = time.Subtract(seedDate);

            int count = (int)Math.Floor(span.TotalDays / (double)interval);

            PayPeriod payPeriod = new PayPeriod();
            payPeriod.Start = seedDate.AddDays(count * interval);

            payPeriod.End = seedDate.AddDays(((count + 1) * interval) - 1);

            return payPeriod;
        }

    }
}