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
        // [GET] /REST/clock/getlist

        /** 
         *  Returns the list of active employees who are able to use the time clock.
         *  
         *  This function is meant for use by embedded devices. The web application will
         *  be accessing this list from the MVC controller (Timeclock/Index) when inially
         *  loading to cut down on open sockets.
         *  
         *  iOS/Android => watch out!
         * 
        **/

        public IEnumerable<ClockInitialItem> GetList()
        {
            using (var db = new TimeClockContext())
            {
                List<ClockInitialItem> employeeList = new List<ClockInitialItem>();

                var activeEmployees = db.Employees.Where(e => e.Terminated == false).OrderBy(e => e.DepartmentID);

                foreach (Employee e in activeEmployees)
                {
                    employeeList.Add(new ClockInitialItem()
                    {
                        EmployeeID = e.EmployeeID,
                        EmployeeName = e.FirstName + " " + e.LastName,
                        DepartmentID = e.DepartmentID
                    });
                }

                return employeeList;
            }
        }

        // [GET] /REST/status/10-Jed

        /** 
         *  Returns the employee's status along with any new/pending messages for them.
         *  The receiving web application should only display the messages after the employee has authenticated.
         * 
         *  Status Code: 200 - Have results
         *               204 - No matching EmployeeID in Employee table
         * 
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

        // [POST] /REST/clock/punch

        /** 
         *  Does the punch!
         * 
         *  Returns a status code of 201 (created) when this is successful.
         * 
         *  Status Code: 201 - Punch successfully created 
         *               204 - EmployeeID & Pin are valid but don't match records in DB
         *               200 - Exception JSON encoded in body example: [{ EmployeeID: "Invalid", EmployeePin: "Invalid - Must be between 4 and 10 digits." }]
         *               
         *  See TimecardTools -> PunchRequest for a definition of the expected POST body.
         *  
        **/

        [HttpPost]
        public HttpResponseMessage Punch(PunchRequest request)
        {
            using (var db = new TimeClockContext())
            {
                /******************************************************************************    TODO ITEM    *********************************/
                //This is where we need to insert the new punch for the employee
                //If it is an out punch, we should recalculate their timecard lines. 

                return new HttpResponseMessage(HttpStatusCode.Created);
            }
        }

        // [POST] /REST/clock/messageviewed

        /** 
         *  Marks a message as viewed. 
         * 
         *  Status Code: 201 - Message archived successfully.
         *               200 - Error JSON encoded in body.
         *  
         *  See TimecardTool -> MessageRead for a definition of the expected POST body
         * 
        **/

        [HttpPost]
        public HttpResponseMessage MessageViewed(MessageRead request)
        {
            using (var db = new TimeClockContext())
            {
                /******************************************************************************    TODO ITEM    *********************************/
                //Need to mark the corresponding message read.

                return new HttpResponseMessage(HttpStatusCode.Created);
            }
        }
    }
}
