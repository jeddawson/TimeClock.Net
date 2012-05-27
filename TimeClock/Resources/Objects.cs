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
          public double Regular { get; set; }
          public double Overtime { get; set; }

         public TimeCardView(int lineNumber, DateTime date, DateTime intime, DateTime outtime, double regular, double overtime) 
          {
            lineNum = lineNumber;
            Date = date;
            In = intime;
            Out = outtime;

            Entry = Out.Subtract(In);

            Regular = regular;
            Overtime = overtime;

          }

         public TimeCardView(int lineNumber, DateTime date, DateTime intime, DateTime outtime)
         {
             lineNum = lineNumber;
             Date = date;
             In = intime;
             Out = outtime;

             Regular = 0;
             Overtime = 0;

             Entry = Out.Subtract(In);
         }

         public void updateEntry() 
         {
            Entry = Out.Subtract(In);
         }



          TimeCardView()
          {
                
               Entry = Out.Subtract(In);
          }
     }
}