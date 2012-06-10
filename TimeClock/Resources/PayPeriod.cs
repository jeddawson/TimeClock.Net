using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TimeClock.Models;

namespace TimeClock.Resources
{
    public class PayPeriodTools
    {
        /* just whipped this up... need to test the logic and make sure we get the first day of the current pay period! */
        public static PayPeriod LookupPayPeriod(TimeClockContext db, int DepartmentID)
        {

            Department depart = db.Departments.SingleOrDefault(d => d.DepartmentID == DepartmentID);

            DateTime seed = depart.PayPeriodSeed;
            int len = depart.PayPeriodInterval;

            TimeSpan duration;

            if (len > 0) duration = TimeSpan.FromDays(len);
            else duration = TimeSpan.MaxValue;

            var now = DateTime.Now;

            while (DateTime.Now.Subtract(seed).TotalMinutes > 0)
                seed.Add(duration);

            seed.Subtract(duration);

            PayPeriod payPeriod = new PayPeriod();
            payPeriod.Start = seed;

            payPeriod.End = seed.Add(duration);

            return payPeriod;

            /* // Before making the interval a timespan instead of an int
           var seed = db.Departments.SingleOrDefault(d => d.DepartmentID == DepartmentID);

           DateTime seedDate = seed.PayPeriodSeed;
           int interval = seed.PayPeriodInterval;

           TimeSpan span = DateTime.Now.Subtract(seedDate);

           int count = (int) Math.Floor(span.TotalDays / (double) interval);
           */
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