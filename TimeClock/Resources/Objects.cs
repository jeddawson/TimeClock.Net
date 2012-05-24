using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeClock.Resources
{
     
     public class PayPeriod
     {
          public DateTime Start { get; set; }
          public DateTime End { get; set; }
     }

     public class TimeCardView
     {
          public int lineNum { get; set; }
          public DateTime Date { get; set; }
          public DateTime In { get; set; }
          public DateTime Out { get; set; }
          public TimeSpan Entry { get; set; }

          TimeCardView()
          {
               Entry = Out.Subtract(In);
          }
     }


}