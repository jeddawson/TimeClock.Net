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

        /**
         * Test function
        **/

        public HttpResponseMessage RebuildLines(string employeeId, string timecardDate = null)
        {
            using (var db = new TimeClockContext())
            {
                var employee = db.Employees.SingleOrDefault(e => e.EmployeeID == employeeId);
                
                if (employee == null)
                    throw new HttpResponseException(HttpStatusCode.BadRequest);

                PayPeriod payperiod = PayPeriodTools.LookupPayPeriod(db, employee.DepartmentID);

                if (timecardDate != null)
                    payperiod = PayPeriodTools.LookupPayPeriod(db, employee.DepartmentID, DateTime.Parse(timecardDate));

                var timecard = employee.Timecards.SingleOrDefault(tc => tc.PayPeriod == payperiod.Start);

                employee.rebuildTimecardLines(db, timecard);

                return new HttpResponseMessage(HttpStatusCode.OK);
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
                Employee emp = db.Employees.FirstOrDefault(e => e.EmployeeID.Equals(request.ID));

                if (!emp.Pin.Equals(request.pin)) // the pin didn't match don't do anything
                {
                    PunchResponse response = new PunchResponse()
                    {
                        isSuccess = false,
                        pinError = "Pin and user did not match.",
                        lines = null,
                        generalError = null
                    };

                    return new HttpResponseMessage<PunchResponse>(response);
                }
                else
                {
                    var payPeriod = PayPeriodTools.LookupPayPeriod(db, emp.DepartmentID);
                    var curTimeCard = emp.Timecards.SingleOrDefault(tc => tc.PayPeriod == payPeriod.Start);

                    PunchResponse retVal = new PunchResponse()
                    {
                        isSuccess = true,
                        pinError = "",
                        generalError = null
                    };

                    if (request.closesPunch == -1)  // Create a new punch in the DB
                    {
                        Punch temp = new Punch()
                        {
                            EmployeeID = emp.EmployeeID,
                            InTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0),
                            OutTime = null,
                            DepartmentID = emp.DepartmentID,
                            PunchTypeID = Constants.DEFAULT_PUNCH_TYPE // Make this equal to the Regular punch type.
                        };

                        db.Punches.Add(temp);
                        db.SaveChanges();

                        var timeCardData = TimeCardView.LinesToTimeCardView(db.Lines.Where(l => l.TimecardID == curTimeCard.TimecardID).OrderBy(l => l.SplitStart));
                        // We add the last line to just display the information, letting the user know that we register the punch in
                        timeCardData.Add(new TimeCardView() 
                        {
                            DateText = DateTime.Now.ToString(@"MM\/dd\/yy"),
                            InText = temp.InTime.ToString(@"hh\:mm"),
                            OutText = "",
                            RegularText = "",
                            OvertimeText = "",
                            DoubletimeText = ""
                        });

                        retVal.lines = timeCardData;
                    }
                    else // We need to close the last punch and make lines -- Make it split the lines over the payperiod seed
                    {
                        // Set the ending time of the punch
                        Punch currentPunch = db.Punches.First(p => p.PunchID == request.closesPunch);
                        currentPunch.OutTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);

                        var timeCardData = db.Lines.Where(l => l.TimecardID == curTimeCard.TimecardID).OrderBy(l => l.SplitStart).ToList();

                        retVal.lines = TimeCardView.LinesToTimeCardView(timeCardData);
                        
                        if (currentPunch.OutTime.Value.Subtract(currentPunch.InTime).TotalMinutes < 1) // punch was shorter than a minut.
                        {
                            db.Punches.Remove(currentPunch);
                            retVal.lines.Add(new TimeCardView() // Make the last line show, but mark is as rapid since it won't get put in the DB
                            {
                                DateText = currentPunch.InTime.ToString(@"MM\/dd\/yy"),
                                InText = currentPunch.InTime.ToString(@"hh\:mm"),
                                OutText = currentPunch.OutTime.Value.ToString(@"hh\:mm"),
                                RegularText = "00:00",
                                OvertimeText = "00:00",
                                DoubletimeText = "00:00",
                                isRapid = true
                            });
                        }
                        else // Punch was longer than a minut, we add it to the DB.
                        {
                            Calculations.addLines(db, currentPunch);
                        }

                        db.SaveChanges();
                    }

                    return new HttpResponseMessage<PunchResponse>(retVal);
                }

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
                MessageViewed temp = db.MessagesViewed.SingleOrDefault(mv => mv.EmployeeID.Equals(request.employeeID) && mv.MessageID.Equals(request.messageID));

                temp.DateViewed = request.time;

                return new HttpResponseMessage(HttpStatusCode.Created);
            }
        }


        // Gets a list of all the messages for an employee.
        public HttpResponseMessage<IEnumerable<Messages>> GetMessages(string id)
        {
            using (var db = new TimeClockContext())
            {
                Employee emp = db.Employees.SingleOrDefault(e => e.EmployeeID.Equals(id));

                var messages = emp.Messages;
                messages.OrderBy(m => m.DateCreated);

                List<Messages> returnMessages = new List<Messages>();

                foreach(Message mes in messages)
                {
                    bool isViewed = true;

                    MessageViewed tempMV = db.MessagesViewed.SingleOrDefault(mv => mv.EmployeeID.Equals(id) && mv.MessageID.Equals(mes.MessageID));

                    if(!tempMV.DateViewed.HasValue)
                        isViewed = false;

                    Messages temp = new Messages() 
                    {
                        MessegeID = mes.MessageID.ToString(),
                        Date = mes.DateCreated.ToString(@"MM\/dd\/yy"),
                        From = mes.Manager.FirstName + " " +  mes.Manager.LastName,
                        Subject = mes.Subject,
                        isViewed = isViewed
                    };

                    returnMessages.Add(temp);
                }
                
                return new HttpResponseMessage<IEnumerable<Messages>>(returnMessages);
            }
        }

        // Get the details of a specific message for an employee.
        public HttpResponseMessage<MessageData> GetMessageDetails(int id, String empId)
        {
            using (var db = new TimeClockContext())
            {
                Message mes = db.Messages.SingleOrDefault(m => m.MessageID == id);
                Employee emp = db.Employees.SingleOrDefault(e => e.EmployeeID == empId);

                MessageData retVal = new MessageData()
                {
                    Date = mes.DateCreated.ToString(@"MM\/dd\/yy"),
                    Subject = mes.Subject,
                    From = mes.Manager.FirstName + " " + mes.Manager.LastName,
                    To = emp.FirstName + " " + emp.LastName,
                    Message = mes.Body
                };

                return new HttpResponseMessage<MessageData>(retVal);
            }
        }

        //Get a list of time cards for an employee
        public HttpResponseMessage<IEnumerable<TimeCardData>> GetTimeCardHistory(string id)
        {
            using (var db = new TimeClockContext())
            {
                var timecards = db.Timecards.Where(tc => tc.EmployeeID.Equals(id));
                timecards.OrderBy(tc => tc.PayPeriod);

                int i = 1;
                List<TimeCardData> retVal = new List<TimeCardData>();

                foreach (Timecard timec in timecards)
                {
                    TimeCardData temp = new TimeCardData()
                    {
                        LineNumber = i.ToString(),
                        StartDate = timec.PayPeriod.ToString(@"MM\/dd\/yy"),
                        EndDate = timec.PayPeriod.AddDays(timec.Employee.department.PayPeriodInterval - 1).ToString(@"MM\/dd\/yy"),
                        TimecardID = timec.TimecardID
                    };

                    retVal.Add(temp);
                }

                return new HttpResponseMessage<IEnumerable<TimeCardData>>(retVal);
            }
        }

        //Should return identical to time card lines
        public HttpResponseMessage<IEnumerable<TimeCardView>> GetTimeCardDetails(string empId, int tcId)
        {
            using (var db = new TimeClockContext())
            {
                Timecard curTimeCard = db.Timecards.SingleOrDefault(tc => tc.TimecardID == tcId);

                var timeCardData = db.Lines.Where(l => l.TimecardID == curTimeCard.TimecardID).OrderBy(l => l.SplitStart).ToList();

                var lines = TimeCardView.LinesToTimeCardView(timeCardData);


                return new HttpResponseMessage<IEnumerable<TimeCardView>>(lines);
            }
        }
    }
}