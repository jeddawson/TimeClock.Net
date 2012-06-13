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
    public class PunchController : Controller
    {
        private TimeClockContext db = new TimeClockContext();

        //
        // GET: /Punch/

        public ViewResult Index(int start = 0, int itemsPerPage = 20, string orderBy = "PunchID", bool desc = false)
        {
            ViewBag.Count = db.Punches.Count();
            ViewBag.Start = start;
            ViewBag.ItemsPerPage = itemsPerPage;
            ViewBag.OrderBy = orderBy;
            ViewBag.Desc = desc;

            return View();
        }

        //
        // GET: /Punch/GridData/?start=0&itemsPerPage=20&orderBy=PunchID&desc=true

        public ActionResult GridData(int start = 0, int itemsPerPage = 20, string orderBy = "PunchID", bool desc = false)
        {
            Response.AppendHeader("X-Total-Row-Count", db.Punches.Count().ToString());
            ObjectQuery<Punch> punches = (db as IObjectContextAdapter).ObjectContext.CreateObjectSet<Punch>();
            punches = (ObjectQuery<Punch>)punches.Include(p => p.employee);
            punches = punches.OrderBy("it." + orderBy + (desc ? " desc" : ""));

            return PartialView(punches.Skip(start).Take(itemsPerPage));
        }

        //
        // GET: /Default5/RowData/5

        public ActionResult RowData(int id)
        {
            Punch punch = db.Punches.Find(id);
            return PartialView("GridData", new Punch[] { punch });
        }

        //
        // GET: /Punch/Create

        public ActionResult Create()
        {
            ViewBag.EmployeeID = new SelectList(db.Employees, "EmployeeID", "FirstName");
            return PartialView("Edit");
        }

        //
        // POST: /Punch/Create

        [HttpPost]
        public ActionResult Create(Punch punch)
        {
            if (ModelState.IsValid)
            {
                db.Punches.Add(punch);
                db.SaveChanges();
                return PartialView("GridData", new Punch[] { punch });
            }

            ViewBag.EmployeeID = new SelectList(db.Employees, "EmployeeID", "FirstName", punch.EmployeeID);
            return PartialView("Edit", punch);
        }

        //
        // GET: /Punch/Edit/5

        public ActionResult Edit(int id)
        {
            Punch punch = db.Punches.Find(id);
            ViewBag.EmployeeID = new SelectList(db.Employees, "EmployeeID", "FirstName", punch.EmployeeID);
            return PartialView(punch);
        }

        //
        // POST: /Punch/Edit/5

        [HttpPost]
        public ActionResult Edit(Punch punch)
        {
            if (ModelState.IsValid)
            {
                db.Entry(punch).State = EntityState.Modified;
                db.SaveChanges();
                return PartialView("GridData", new Punch[] { punch });
            }

            ViewBag.EmployeeID = new SelectList(db.Employees, "EmployeeID", "FirstName", punch.EmployeeID);
            return PartialView(punch);
        }

        //
        // POST: /Punch/Delete/5

        [HttpPost]
        public void Delete(int id)
        {
            Punch punch = db.Punches.Find(id);
            db.Punches.Remove(punch);
            db.SaveChanges();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
