using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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

    }
}
