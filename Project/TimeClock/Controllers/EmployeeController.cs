using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimeClock.Models;

namespace TimeClock.Controllers
{
    public class EmployeeController : Controller
    {
        private TimeClockContext db = new TimeClockContext();

        //
        // GET: /Employee/

        public ViewResult Index(int start = 0, int itemsPerPage = 20, string orderBy = "EmployeeID", bool desc = false)
        {
            ViewBag.Count = db.Employees.Count();
            ViewBag.Start = start;
            ViewBag.ItemsPerPage = itemsPerPage;
            ViewBag.OrderBy = orderBy;
            ViewBag.Desc = desc;

            return View();
        }

        //
        // GET: /Employee/GridData/?start=0&itemsPerPage=20&orderBy=EmployeeID&desc=true

        public ActionResult GridData(int start = 0, int itemsPerPage = 20, string orderBy = "EmployeeID", bool desc = false)
        {
            Response.AppendHeader("X-Total-Row-Count", db.Employees.Count().ToString());
            ObjectQuery<Employee> employees = (db as IObjectContextAdapter).ObjectContext.CreateObjectSet<Employee>();
            employees = employees.OrderBy("it." + orderBy + (desc ? " desc" : ""));

            return PartialView(employees.Skip(start).Take(itemsPerPage));
        }

        //
        // GET: /Default5/RowData/5

        public ActionResult RowData(string id)
        {
            Employee employee = db.Employees.Find(id);
            return PartialView("GridData", new Employee[] { employee });
        }

        //
        // GET: /Employee/Create

        public ActionResult Create()
        {
            return PartialView("Edit");
        }

        //
        // POST: /Employee/Create

        [HttpPost]
        public ActionResult Create(Employee employee)
        {
            if (ModelState.IsValid)
            {
                db.Employees.Add(employee);
                db.SaveChanges();
                return PartialView("GridData", new Employee[] { employee });
            }

            return PartialView("Edit", employee);
        }

        //
        // GET: /Employee/Edit/5

        public ActionResult Edit(string id)
        {
            Employee employee = db.Employees.Find(id);
            return PartialView(employee);
        }

        //
        // POST: /Employee/Edit/5

        [HttpPost]
        public ActionResult Edit(Employee employee)
        {
            if (ModelState.IsValid)
            {
                db.Entry(employee).State = EntityState.Modified;
                db.SaveChanges();
                return PartialView("GridData", new Employee[] { employee });
            }

            return PartialView(employee);
        }

        //
        // POST: /Employee/Delete/5

        [HttpPost]
        public void Delete(string id)
        {
            Employee employee = db.Employees.Find(id);
            db.Employees.Remove(employee);
            db.SaveChanges();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
