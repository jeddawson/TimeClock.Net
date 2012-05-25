using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimeClock.Models;
using TimeClock.Resources;

namespace TimeClock.Controllers
{
    public class TimeclockController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        /* Looks up the employees current status
         */
        public ActionResult Status(string id)
        {
            using (var db = new TimeClockContext())
            {
                PayPeriod payPeriod = LookupPayPeriod();

                var empPunches = db.Punches.Where(p => p.EmployeeID == id && p.InTime > payPeriod.Start && p.InTime < payPeriod.End);
                if (empPunches.Count(p => p.OutTime == null) != 0)
                    ViewBag.PunchDirection = "In";
                else
                    ViewBag.PunchDirection = "Out";

                return PartialView();
            }
        }

        public ActionResult Timecard(string id)
        {
            using (var db = new TimeClockContext())
            {
                int lineNumberCounter = 1;
                List<TimeCardView> timecard = new List<TimeCardView>();
                PayPeriod payPeriod = LookupPayPeriod();

                var empTC = db.Timecards.SingleOrDefault(t => t.EmployeeID == id && t.PayPeriod == payPeriod.Start);

                var lines = db.Lines.Where(l => l.TimecardID == empTC.TimecardID);

                lines.OrderBy(l => l.SplitStart);

                foreach (Line line in lines)
                {
                    // If the previous line contains information about the same punch, the we update it.
                    if (timecard.Count > 0 && timecard[timecard.Count - 1].Out == line.SplitStart) 
                    {
                        timecard[timecard.Count - 1].Out = line.SplitEnd;
                        timecard[timecard.Count - 1].Overtime = line.SplitStart.Subtract(line.SplitEnd).TotalHours;
                        timecard[timecard.Count - 1].updateEntry();
                    }
                    // Otherwise we create a new line and add it to the timecard.
                    else
                    {
                        TimeCardView temp = new TimeCardView(lineNumberCounter, line.SplitStart.Date, line.SplitStart, line.SplitEnd);
                        if (line.PayType.Description == "Regular")
                            temp.Regular = line.SplitStart.Subtract(line.SplitEnd).TotalHours;
                        else if (line.PayType.Description == "Overtime")
                            temp.Overtime = line.SplitStart.Subtract(line.SplitEnd).TotalHours;
                        else
                            ;// What should we do if it is neither of the two?

                        timecard.Add(temp);
                    }

                }

                return PartialView(timecard);
            }
        }

    }
}
