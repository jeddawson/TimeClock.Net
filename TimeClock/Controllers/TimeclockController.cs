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
        //Need this context for views that require persistent connection (when pulling data at time of view creation)
        private TimeClockContext dbContext = new TimeClockContext();

        public ActionResult Index()
        {
            List<ClockInitialItem> initialData = new List<ClockInitialItem>();

            var activeEmployees = dbContext.Employees.Where(e => e.Terminated == false).OrderBy(e => e.DepartmentID);

            foreach (Employee e in activeEmployees)
            {
                initialData.Add(new ClockInitialItem()
                {
                    EmployeeID = e.EmployeeID,
                    EmployeeName = e.FirstName + " " + e.LastName,
                    DepartmentID = e.DepartmentID
                });
            }
   
            ViewBag.EmpList = initialData.AsEnumerable();
            return View(initialData);
        }

        /* Looks up the employees current status
         */
        public ActionResult Status(string id)
        {
            using (var db = new TimeClockContext())
            {
                Employee emp = db.Employees.FirstOrDefault(e => e.EmployeeID == id);
                bool empIn = emp.isWorking(db);
    
                if (empIn)
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
                PayPeriod payPeriod = PayPeriodTools.LookupPayPeriod(db, 1);

                var empTC = db.Timecards.SingleOrDefault(t => t.EmployeeID == id && t.PayPeriod == payPeriod.Start);

                var lines = db.Lines.Where(l => l.TimecardID == empTC.TimecardID);

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
                        else ;

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
