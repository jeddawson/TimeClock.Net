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
    public class DepartmentController : Controller
    {
        private TimeClockContext db = new TimeClockContext();

        //
        // GET: /Department/

        public ViewResult Index(int start = 0, int itemsPerPage = 20, string orderBy = "DepartmentID", bool desc = false)
        {
            ViewBag.Count = db.Departments.Count();
            ViewBag.Start = start;
            ViewBag.ItemsPerPage = itemsPerPage;
            ViewBag.OrderBy = orderBy;
            ViewBag.Desc = desc;

            return View();
        }

        //
        // GET: /Department/GridData/?start=0&itemsPerPage=20&orderBy=DepartmentID&desc=true

        public ActionResult GridData(int start = 0, int itemsPerPage = 20, string orderBy = "DepartmentID", bool desc = false)
        {
            Response.AppendHeader("X-Total-Row-Count", db.Departments.Count().ToString());
            ObjectQuery<Department> departments = (db as IObjectContextAdapter).ObjectContext.CreateObjectSet<Department>();
            departments = departments.OrderBy("it." + orderBy + (desc ? " desc" : ""));

            return PartialView(departments.Skip(start).Take(itemsPerPage));
        }

        //
        // GET: /Default5/RowData/5

        public ActionResult RowData(int id)
        {
            Department department = db.Departments.Find(id);
            return PartialView("GridData", new Department[] { department });
        }

        //
        // GET: /Department/Create

        public ActionResult Create()
        {
            return PartialView("Edit");
        }

        //
        // POST: /Department/Create

        [HttpPost]
        public ActionResult Create(Department department)
        {
            if (ModelState.IsValid)
            {
                db.Departments.Add(department);
                db.SaveChanges();
                return PartialView("GridData", new Department[] { department });
            }

            return PartialView("Edit", department);
        }

        //
        // GET: /Department/Edit/5

        public ActionResult Edit(int id)
        {
            Department department = db.Departments.Find(id);
            return PartialView(department);
        }

        //
        // POST: /Department/Edit/5

        [HttpPost]
        public ActionResult Edit(Department department)
        {
            if (ModelState.IsValid)
            {
                db.Entry(department).State = EntityState.Modified;
                db.SaveChanges();
                return PartialView("GridData", new Department[] { department });
            }

            return PartialView(department);
        }

        //
        // POST: /Department/Delete/5

        [HttpPost]
        public void Delete(int id)
        {
            Department department = db.Departments.Find(id);
            db.Departments.Remove(department);
            db.SaveChanges();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
