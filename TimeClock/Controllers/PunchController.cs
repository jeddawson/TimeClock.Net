using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimeClock.Models;
using TimeClock.Resources;


namespace TimeClock.Controllers
{
    public class PunchController : Controller
    {
        //
        // GET: /Punch/

        public ActionResult Index()
        {
            return View();
        }


        public ActionResult PunchGrid(string EID, PayPeriod pay = null)
        {
            using (var db = new TimeClockContext())
            {
                if (pay == null)
                    pay = PayPeriodTools.LookupPayPeriod(db, 1);

                // How are we suppose to handle punches that extend over the end of a payperiod?
                //
                // Get all punches from p payperiod - into a list
                List<Punch> punches = db.Punches.Where(p => p.EmployeeID == EID && pay.Start < p.InTime && p.InTime < pay.End).ToList();


                // return partial view
                return PartialView("PunchData", punches);
            }
        }


    }
}
