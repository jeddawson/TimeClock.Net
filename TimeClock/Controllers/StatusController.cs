using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimeClock.Models;
using TimeClock.Resources;

namespace TimeClock.Controllers
{
    public class StatusController : Controller
    {
        //
        // GET: /Status/

        private TimeClockContext dbContext = new TimeClockContext();

        public ActionResult Index()
        {
            
            using (var db = new TimeClockContext())
            {
                List<Employee> WorkingEmp = new List<Employee>();

                var Emp = db.Employees.Where(e => e.Terminated == false);

                foreach(Employee employees in Emp)
                {
                    if (employees.isWorking(db))
                        WorkingEmp.Add(employees);
                }


                return View(WorkingEmp);                
            }


        }

    }
}
