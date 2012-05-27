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
    public class CompanyController : Controller
    {
        private TimeClockContext db = new TimeClockContext();

        //
        // GET: /Company/

        public ViewResult Index(int start = 0, int itemsPerPage = 20, string orderBy = "CompanyID", bool desc = false)
        {
            ViewBag.Count = db.Companies.Count();
            ViewBag.Start = start;
            ViewBag.ItemsPerPage = itemsPerPage;
            ViewBag.OrderBy = orderBy;
            ViewBag.Desc = desc;

            return View();
        }

        //
        // GET: /Company/GridData/?start=0&itemsPerPage=20&orderBy=CompanyID&desc=true

        public ActionResult GridData(int start = 0, int itemsPerPage = 20, string orderBy = "CompanyID", bool desc = false)
        {
            Response.AppendHeader("X-Total-Row-Count", db.Companies.Count().ToString());
            ObjectQuery<Company> companies = (db as IObjectContextAdapter).ObjectContext.CreateObjectSet<Company>();
            companies = companies.OrderBy("it." + orderBy + (desc ? " desc" : ""));

            return PartialView(companies.Skip(start).Take(itemsPerPage));
        }

        //
        // GET: /Default5/RowData/5

        public ActionResult RowData(int id)
        {
            Company company = db.Companies.Find(id);
            return PartialView("GridData", new Company[] { company });
        }

        //
        // GET: /Company/Create

        public ActionResult Create()
        {
            return PartialView("Edit");
        }

        //
        // POST: /Company/Create

        [HttpPost]
        public ActionResult Create(Company company)
        {
            if (ModelState.IsValid)
            {
                db.Companies.Add(company);
                db.SaveChanges();
                return PartialView("GridData", new Company[] { company });
            }

            return PartialView("Edit", company);
        }

        //
        // GET: /Company/Edit/5

        public ActionResult Edit(int id)
        {
            Company company = db.Companies.Find(id);
            return PartialView(company);
        }

        //
        // POST: /Company/Edit/5

        [HttpPost]
        public ActionResult Edit(Company company)
        {
            if (ModelState.IsValid)
            {
                db.Entry(company).State = EntityState.Modified;
                db.SaveChanges();
                return PartialView("GridData", new Company[] { company });
            }

            return PartialView(company);
        }

        //
        // POST: /Company/Delete/5

        [HttpPost]
        public void Delete(int id)
        {
            Company company = db.Companies.Find(id);
            db.Companies.Remove(company);
            db.SaveChanges();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
