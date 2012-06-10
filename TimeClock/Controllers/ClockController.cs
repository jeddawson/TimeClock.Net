using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq.SqlClient;
using System.Net.Http;
using System.Web.Http;
using TimeClock.Resources;
using TimeClock.Models;
using System.Net;

namespace TimeClock.Controllers
{

    public class ClockController : ApiController
    {
        
        /*
        private IEnumerable<PayType> PayTypes = new TimeClockContext().PayTypes.Where(pt => 
               pt.Description.Equals("Regular") || 
               pt.Description.Equals("Overtime")
               ).AsEnumerable();
        */


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

        public IEnumerable<ClockInitialItem> List()
        {
            using (var db = new TimeClockContext())
            {
                List<ClockInitialItem> employeeList = new List<ClockInitialItem>();

                var activeEmployees = db.Employees.Where(e => e.Terminated == false).OrderBy(e => e.DepartmentID);

                foreach (Employee e in activeEmployees)
                {
                    employeeList.Add(new ClockInitialItem()
                    {
                        employeeID      =   e.EmployeeID,
                        employeeName    =   e.FirstName + " " + e.LastName,
                        departmentID    =   e.DepartmentID
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

                //PayPeriod payPeriod = PayPeriodTools.LookupPayPeriod(db, Constants.DEFAULT_DEPARTMENT);

                if (employee == null)
                    throw new HttpResponseException(HttpStatusCode.NoContent);

                var isWorking = employee.isWorking(db);

                EmployeeStatus status = new EmployeeStatus()
                    {
                        EmployeeID  = id,
                        punchDirection = (isWorking >= 0 ? "Punch Out" : "Punch In"),
                        openPunch   = isWorking,
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
        public HttpResponseMessage<PunchResponse> Punch(PunchRequest request)
        {
            using (var db = new TimeClockContext())
            {
                /******************************************************************************    TODO ITEM    *********************************/
                //This is where we need to insert the new punch for the employee
                //If it is an out punch, we should recalculate their timecard lines. 
                Employee emp = db.Employees.FirstOrDefault(e => e.EmployeeID.Equals(request.ID));

                if (!emp.Pin.Equals(request.pin))
                {
                    PunchResponse response = new PunchResponse()
                    {
                        isSuccess = false,
                        pinError = "Pin and user did not match.",
                        timecardData = null,
                        generalError = null
                    };

                    return new HttpResponseMessage<PunchResponse>(response);

                }
                else
                {
                    if (request.closesPunch == -1)  // Create a new punch in the DB
                    {
                        Punch temp = new Punch()
                        {
                            EmployeeID = emp.EmployeeID,
                            InTime = DateTime.Now,
                            OutTime = null,
                            DepartmentID = emp.DepartmentID,
                            PunchTypeID = Constants.DEFAULT_PUNCH_TYPE // Make this equal to the Regular punch type.
                            //OutTime = null;
                        };

                        db.Punches.Add(temp);
                        db.SaveChanges();
                    }
                    else // We need to close the last punch and make lines -- Make it split the lines over the payperiod seed
                    {
                        // Set the ending time of the punch
                        Punch currentPunch = db.Punches.First(p => p.PunchID == request.closesPunch);
                        currentPunch.OutTime = DateTime.Now;
                        db.SaveChanges();

                        // Update the lines for this punch 

                        PayPeriod currentPayPeriod = PayPeriodTools.LookupPayPeriod(db, emp.department.DepartmentID, currentPunch.InTime);
                        Timecard empTimecard = db.Timecards.SingleOrDefault(tc => tc.EmployeeID.Equals(emp.EmployeeID) && tc.PayPeriod == currentPayPeriod.Start);

                        //If the timecard doesn't exist we need to create one
                        if (empTimecard == null)
                        {
                            db.Timecards.Add(new Timecard() { EmployeeID = emp.EmployeeID, PayPeriod = currentPayPeriod.Start });
                            db.SaveChanges();
                            empTimecard = db.Timecards.SingleOrDefault(tc => tc.EmployeeID.Equals(emp.EmployeeID) && tc.PayPeriod == currentPayPeriod.Start);
                        }

                        PayType payType = emp.department.DefaultPayType;

                        var lines = db.Lines.Where(l => l.TimecardID == empTimecard.TimecardID);

                        //Need to do something if lines is null!

                        double weeklyMinutes = 0;
                        foreach (Line line in lines)
                            weeklyMinutes += line.getDuration().TotalMinutes;

                        while (weeklyMinutes > payType.WeeklyMax)
                            payType = payType.NextPayType;

                        double dailyMinutes = 0; // This needs to be changed.

                        /* Linq wasn't able to interprety this. Nothing wrong with your syntax, linq just isn't quite there yet.
                         *  foreach (Line line in lines.Where(l =>
                            l.SplitStart.Subtract(currentPunch.InTime).Days == 0 || 
                            l.SplitStart.Subtract(currentPunch.InTime).Days == 1)
                            )
                         */

                        /* Got documenation on DateDiffDay here: http://msdn.microsoft.com/en-us/library/bb468730(v=vs.110).aspx */

                        var linesQuery = from l in lines
                                         where SqlMethods.DateDiffDay(l.SplitStart, currentPunch.InTime) == 0
                                            select l;

                        foreach (Line line in linesQuery)
                        {
                            dailyMinutes += line.getDuration().TotalMinutes;
                        }

                        while (dailyMinutes > payType.DailyMax)
                            payType = payType.NextPayType;

                        
                        if (currentPunch.OutTime < currentPayPeriod.Start.AddDays((double)emp.department.PayPeriodInterval)) // We don't need to split the punch over pay periods
                        {
                            double minutsWorkd = currentPunch.getDuration().TotalMinutes;
                            double dailyMinutsLeft = payType.DailyMax - dailyMinutes;
                            double weeklyMinutsLeft = payType.WeeklyMax - weeklyMinutes;



                            /*
                            if (weeklyMinuts + currentPunch.getDuration().TotalMinutes < payType.WeeklyMax) // No need to split due to weekly max
                            {
                                if (dailyMinuts + currentPunch.getDuration().TotalMinutes < payType.DailyMax) // No need to split due to daily max
                                {
                                    db.Lines.Add(new Line() 
                                        { 
                                        TimecardID = empTimecard.TimecardID,
                                        PunchID = currentPunch.PunchID,
                                        SplitStart = currentPunch.InTime,
                                        SplitEnd = currentPunch.OutTime.Value,
                                        }
                                        );
                                    db.SaveChanges();
                                }

                            }
                            */
                        }

                    }

                }

                PunchResponse retVal = new PunchResponse()
                {
                    isSuccess = true,
                    pinError = "",
                    timecardData = null,
                    generalError = null
                };

                return new HttpResponseMessage<PunchResponse>(retVal);
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