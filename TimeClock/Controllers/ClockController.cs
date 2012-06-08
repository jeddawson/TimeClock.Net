using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using TimeClock.Resources;
using TimeClock.Models;
using System.Net;

namespace TimeClock.Controllers
{
    public class ClockController : ApiController
    {
        // GET /api/clock

        /** 
         *  Returns the current server time in JSON. No real point to this operation.
        **/

        public String Get()
        {
            return "[{ ServerTime: \"" + DateTime.Now + "\" }]";
        }

        // GET /REST/status/10-Jed

        /** 
         *  Returns the employee's status along with any new/pending messages for them. 
        **/

        public EmployeeStatus Status(string id)
        {
            using( var db = new TimeClockContext() ) {

                Employee employee = db.Employees.SingleOrDefault(e => e.EmployeeID == id);

                PayPeriod payPeriod = PayPeriodTools.LookupPayPeriod(db, 1);

                if (employee == null)
                    throw new HttpResponseException(HttpStatusCode.NoContent);

                EmployeeStatus status = new EmployeeStatus()
                    {
                        EmployeeID = id,
                        openPunch = employee.isWorking(db),
                        NewMessages = employee.PendingMessages(db),
                        Timecard = employee.getTimeCardLines(db, payPeriod)
                    };
                
                return status;
            }
        }

        // POST /REST/clock/punch

        /** 
         *  Does the punch!
         * 
         *  Returns a status code of 201 (created) when this is successful.
         *  Otherwise throws an error, with explaination.
         * 
        **/

        [HttpPost]
        public HttpResponseMessage Punch(PunchRequest request)
        {
            using (var db = new TimeClockContext())
            {
                //TODO ITEM
                //This is where we need to insert the new punch for the employee
                //If it is an out punch, we should recalculate their timecard lines. 

                return new HttpResponseMessage(HttpStatusCode.Created);
            }
        }

        // POST /REST/clock/messageviewed

        /** 
         *  Marks a message as viewed. 
         * 
         *  Returns a status code of 201 (created) when this is successful.
         *  Otherwise throws an error, with explaination.
         * 
        **/

        [HttpPost]
        public HttpResponseMessage MessageViewed(MessageRead request)
        {
            using (var db = new TimeClockContext())
            {
                //TODO ITEM
                //Need to mark the corresponding message read.

                return new HttpResponseMessage(HttpStatusCode.Created);
            }
        }
    }
}
