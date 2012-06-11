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
    public class PayTypeController : Controller
    {
        private TimeClockContext db = new TimeClockContext();

        //
        // GET: /PayType/

        public ActionResult Index()
        {
            return View(db.PayTypes.ToList());
        }

        //
        // GET: /PayType/Details/5

        public ActionResult Details(int id = 0)
        {
            PayType paytype = db.PayTypes.Find(id);
            if (paytype == null)
            {
                return HttpNotFound();
            }
            return View(paytype);
        }

        //
        // GET: /PayType/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /PayType/Create

        [HttpPost]
        public ActionResult Create(PayType paytype)
        {
            if (ModelState.IsValid)
            {
                db.PayTypes.Add(paytype);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(paytype);
        }

        //
        // GET: /PayType/Edit/5

        public ActionResult Edit(int id = 0)
        {
            PayType paytype = db.PayTypes.Find(id);
            if (paytype == null)
            {
                return HttpNotFound();
            }
            return View(paytype);
        }

        //
        // POST: /PayType/Edit/5

        [HttpPost]
        public ActionResult Edit(PayType paytype)
        {
            if (ModelState.IsValid)
            {
                db.Entry(paytype).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(paytype);
        }

        //
        // GET: /PayType/Delete/5

        public ActionResult Delete(int id = 0)
        {
            PayType paytype = db.PayTypes.Find(id);
            if (paytype == null)
            {
                return HttpNotFound();
            }
            return View(paytype);
        }

        //
        // POST: /PayType/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            PayType paytype = db.PayTypes.Find(id);
            db.PayTypes.Remove(paytype);
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