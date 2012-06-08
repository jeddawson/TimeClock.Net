using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TimeClock.Models;

namespace TimeClock.Resources
{
    public class EmployeeStatus
    {
        public string EmployeeID { get; set; }
        public bool isWorking { get; set; }
        public IEnumerable<Message> NewMessages { get; set; }
        public IEnumerable<TimeCardView> Timecard { get; set; }
    }

    public class PunchRequest
    {
        public string Id { get; set; }
        public string Pin { get; set; }
        public string Timestamp { get; set; }
        public string HMAC { get; set; }
    }

    public class ClockInitialItem
    {
        public string EmployeeName { get; set; }
        public string EmployeeID { get; set; }
        public int DepartmentID { get; set; }
    }

    public class MessageRead
    {
        public int MessageID { get; set; }
        public string EmployeeID { get; set; }
    }

}