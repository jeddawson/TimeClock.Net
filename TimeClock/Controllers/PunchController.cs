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


        public ActionResult PunchGrid(string EID, PayPeriod p = null)
        {
            using (var db = new TimeClockContext())
            {
                if (p == null)
                    p = PayPeriodTools.LookupPayPeriod(db, 1);

                // Get all punches from p payperiod - into a list
                List<Punch> punches = null;


                // return partial view
                return PartialView("GridData", punches);
            }
        }


    }
}
