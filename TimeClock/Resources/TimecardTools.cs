using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TimeClock.Models;


/**
 *  Serializable Helper Objects
 *  
 *  The objects in this file will be used to serialize the inbound and outbound data.
 * 
**/

namespace TimeClock.Resources
{
       
    public class EmployeeStatus
    {
        public string EmployeeID { get; set; }
        public int openPunch { get; set; }
        public IEnumerable<Message> NewMessages { get; set; }
        public IEnumerable<TimeCardView> Timecard { get; set; }
    }


    /**
     *  Inbound Request - Punch Request Definition
     *  
     *  Serializable class that allows JSON/XML objects to be serialized in the HTTP POST body.
     *  Object should look like: [{ Id: "", Pin: "", closesPunch: 0, Timestamp: "", HMAC: "" }]
     * 
     *  Any field can be ommited and the object will still serialize, however the relying code
     *  may not perform as expected.
     * 
    **/

    public class PunchRequest
    {
        public string Id { get; set; }
        public string Pin { get; set; }
        public int closesPunch { get; set; }
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