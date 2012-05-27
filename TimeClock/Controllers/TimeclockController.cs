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
        //
        // GET: /Timeclock/

        public ActionResult Index()
        {
            return View();
        }
         /*
          /* Looks up the employees current status
           *
        public ActionResult Status(string id)
        {
             using (var db = new TimeClockContext())
             {
                  PayPeriod payPeriod = LookupPayPeriod();

                  var empPunches = db.Punches.Where(p => p.EmployeeID == id && p.InTime > payPeriod.Start && p.InTime < payPeriod.End);

                  foreach( Punch punch in empPunches) {
                       
                  }

                  if(should be in)
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

                  List<Line> timecard = new List<Line>();
                  PayPeriod payPeriod = LookupPayPeriod();

                  var empTC = db.Timecards.SingleOrDefault( t => t.EmployeeID == id && t.PayPeriod == payPeriod.Start);

                  var lines = db.Lines.Where(l => l.TimecardID == empTC.TimecardID);

                  lines.OrderBy(l => l.SplitStart);

                  foreach (Line line in lines)
                  {
                       line.PayType.
                  }

                  return PartialView(timecard);
             }
        }
         */
    }
}
