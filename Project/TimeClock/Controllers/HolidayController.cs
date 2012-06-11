using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimeClock.Models;

namespace TimeClock.Controllers
{
    public class HolidayController : Controller
    {
        private TimeClockContext db = new TimeClockContext();

        //
        // GET: /Holiday/

        public ActionResult Index()
        {
            return View(db.Holidays.ToList());
        }

        //
        // GET: /Holiday/Details/5

        public ActionResult Details(int id = 0)
        {
            Holiday holiday = db.Holidays.Find(id);
            if (holiday == null)
            {
                return HttpNotFound();
            }
            return View(holiday);
        }

        //
        // GET: /Holiday/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Holiday/Create

        [HttpPost]
        public ActionResult Create(Holiday holiday)
        {
            if (ModelState.IsValid)
            {
                db.Holidays.Add(holiday);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(holiday);
        }

        //
        // GET: /Holiday/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Holiday holiday = db.Holidays.Find(id);
            if (holiday == null)
            {
                return HttpNotFound();
            }
            return View(holiday);
        }

        //
        // POST: /Holiday/Edit/5

        [HttpPost]
        public ActionResult Edit(Holiday holiday)
        {
            if (ModelState.IsValid)
            {
                db.Entry(holiday).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(holiday);
        }

        //
        // GET: /Holiday/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Holiday holiday = db.Holidays.Find(id);
            if (holiday == null)
            {
                return HttpNotFound();
            }
            return View(holiday);
        }

        //
        // POST: /Holiday/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Holiday holiday = db.Holidays.Find(id);
            db.Holidays.Remove(holiday);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}