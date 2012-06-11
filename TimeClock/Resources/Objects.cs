using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TimeClock.Models;

namespace TimeClock.Resources
{
     
     public class PayPeriod
     {
          public DateTime Start { get; set; }
          public DateTime End { get; set; }

          public bool Equals(PayPeriod p)
          {
              if ((object)p == null)
                  return false;

              return (Start.Equals(p.Start) && End.Equals(p.End));
          }
     }

     public class TimeCardView
     {
          public int lineNum { get; set; }
          public DateTime Date { get; set; }
          public DateTime In { get; set; }
          public DateTime Out { get; set; }
          public string Entry { get; set; }
          public double Regular { get; set; }
          public double Overtime { get; set; }
         public enum PunchTypes {REGULAR, HOLIDAY, SICK, VACATION}
         public PunchTypes PunchType {get; set; }
         public int PunchID { get; set; }

         public static IEnumerable<TimeCardView> LinesToTimeCardView(IEnumerable<Line> tcLines)
         {
             List<TimeCardView> view = new List<TimeCardView>();
             int i = 0;
             double regular = 0;
             double overtime = 0;
             double doubletime = 0;
             PunchTypes pType = new PunchTypes();
             foreach (Line line in tcLines)
             {
                 switch (line.PayType.Description)
                 {
                     case "Regular":
                         regular += line.SplitEnd.Subtract(line.SplitStart).TotalMinutes;
                         break;
                     case "Overtime":
                         overtime += line.SplitEnd.Subtract(line.SplitStart).TotalMinutes;
                         break;
                     case "Doubletime":
                         doubletime += line.SplitEnd.Subtract(line.SplitStart).TotalMinutes;
                         break;
                 }

                 switch (line.Punch.PunchTypeID)
                 {
                     case 1:
                         pType = PunchTypes.REGULAR;
                         break;
                     case 2:
                         pType = PunchTypes.SICK;
                         break;
                     case 3:
                         pType = PunchTypes.VACATION;
                         break;
                     case 4:
                         pType = PunchTypes.HOLIDAY;
                         break;
                 }
                 
                 var single = new TimeCardView()
                 {
                     lineNum = i,
                     Date = line.SplitStart.Date,
                     In = line.SplitStart,
                     Out = line.SplitEnd,
                     Entry = line.SplitEnd.Subtract(line.SplitStart).ToString(@"hh\:mm"),
                     Regular = regular,
                     Overtime = overtime,
                     PunchType = pType,
                     PunchID = line.PunchID
                 };

                 view.Add(single);
             }

             return view.AsEnumerable();
         }

         public TimeCardView(int lineNumber, DateTime date, DateTime intime, DateTime outtime, double regular, double overtime, int punchID, PunchTypes punchType = PunchTypes.REGULAR) 
          {
            lineNum = lineNumber;
            Date = date;
            In = intime;
            Out = outtime;

            Entry = Out.Subtract(In).ToString(@"hh\:mm");

            Regular = regular;
            Overtime = overtime;

            PunchID = punchID;
            PunchType = punchType;

          }

         public TimeCardView(int lineNumber, DateTime date, DateTime intime, DateTime outtime, int punchID, PunchTypes punchType = PunchTypes.REGULAR)
         {
             lineNum = lineNumber;
             Date = date;
             In = intime;
             Out = outtime;

             Regular = 0;
             Overtime = 0;

             Entry = Out.Subtract(In).ToString(@"hh\:mm");

             PunchType = punchType;
             PunchID = punchID;
         }

         public void updateEntry() 
         {
             Entry = Out.Subtract(In).ToString(@"hh\:mm");
         }



          TimeCardView()
          {

              Entry = Out.Subtract(In).ToString(@"hh\:mm");
          }
     }
}