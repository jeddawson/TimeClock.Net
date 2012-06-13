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
    public class MessageController : Controller
    {
        private TimeClockContext db = new TimeClockContext();

        //
        // GET: /Message/

        public ViewResult Index(int start = 0, int itemsPerPage = 20, string orderBy = "MessageID", bool desc = false)
        {
            ViewBag.Count = db.Messages.Count();
            ViewBag.Start = start;
            ViewBag.ItemsPerPage = itemsPerPage;
            ViewBag.OrderBy = orderBy;
            ViewBag.Desc = desc;

            return View();
        }

        //
        // GET: /Message/GridData/?start=0&itemsPerPage=20&orderBy=MessageID&desc=true

        public ActionResult GridData(int start = 0, int itemsPerPage = 20, string orderBy = "MessageID", bool desc = false)
        {
            Response.AppendHeader("X-Total-Row-Count", db.Messages.Count().ToString());
            ObjectQuery<Message> messages = (db as IObjectContextAdapter).ObjectContext.CreateObjectSet<Message>();
            messages = messages.OrderBy("it." + orderBy + (desc ? " desc" : ""));

            return PartialView(messages.Skip(start).Take(itemsPerPage));
        }

        //
        // GET: /Default5/RowData/5

        public ActionResult RowData(int id)
        {
            Message message = db.Messages.Find(id);
            return PartialView("GridData", new Message[] { message });
        }

        //
        // GET: /Message/Create

        public ActionResult Create()
        {
            return PartialView("Edit");
        }

        //
        // POST: /Message/Create

        [HttpPost]
        public ActionResult Create(Message message)
        {
            if (ModelState.IsValid)
            {
                db.Messages.Add(message);
                db.SaveChanges();
                return PartialView("GridData", new Message[] { message });
            }

            return PartialView("Edit", message);
        }

        //
        // GET: /Message/Edit/5

        public ActionResult Edit(int id)
        {
            Message message = db.Messages.Find(id);
            return PartialView(message);
        }

        //
        // POST: /Message/Edit/5

        [HttpPost]
        public ActionResult Edit(Message message)
        {
            if (ModelState.IsValid)
            {
                db.Entry(message).State = EntityState.Modified;
                db.SaveChanges();
                return PartialView("GridData", new Message[] { message });
            }

            return PartialView(message);
        }

        //
        // POST: /Message/Delete/5

        [HttpPost]
        public void Delete(int id)
        {
            Message message = db.Messages.Find(id);
            db.Messages.Remove(message);
            db.SaveChanges();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
