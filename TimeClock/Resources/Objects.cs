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
         public String DateText { get; set; }
         public DateTime In { get; set; }
         public String InText { get; set; }
         public DateTime Out { get; set; }
         public String OutText { get; set; }
         public TimeSpan Entry { get; set; }
         public String EntryText { get; set; }
         public double Regular { get; set; }
         public String RegularText { get; set; }
         public double Overtime { get; set; }
         public String OvertimeText { get; set; }
         public double Doubletime { get; set; }
         public String DoubletimeText { get; set; }
         public enum PunchTypes { REGULAR, HOLIDAY, SICK, VACATION }
         public PunchTypes PunchType { get; set; }
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
                 regular = 0;
                 overtime = 0;
                 doubletime = 0;

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
                     Entry = line.SplitEnd.Subtract(line.SplitStart),
                     Regular = regular,
                     Overtime = overtime,
                     Doubletime = doubletime,
                     PunchType = pType,
                     PunchID = line.PunchID,

                 };

                 single.generateText();

                 view.Add(single);
             }

             return view.AsEnumerable();
         }

         private void generateText()
         {
             DateText = Date.ToString(@"MM\/dd\/yy");
             InText = In.ToString(@"hh\:mm");
             OutText = Out.ToString(@"hh\:mm");
             EntryText = parseDoubleToHoursMinutes(Entry.TotalMinutes);//Entry.ToString(@"hh\:mm");
             RegularText = parseDoubleToHoursMinutes(Regular);//TimeSpan.FromMinutes(Math.Round(Regular, 2)).ToString(@"hh\:mm");
             OvertimeText = parseDoubleToHoursMinutes(Overtime);//TimeSpan.FromMinutes(Math.Round(Overtime, 2)).ToString(@"hh\:mm");
             DoubletimeText = parseDoubleToHoursMinutes(Doubletime);//TimeSpan.FromMinutes(Math.Round(Doubletime, 2)).ToString(@"hh\:mm");
         }

         private String parseDoubleToHoursMinutes(double time)
         {
             String hours = ((int) time / 60).ToString("D2");
             String minutes = ((int)Math.Round(time % 60)).ToString("D2");

             return hours + ":" + minutes;
         }


         public TimeCardView(int lineNumber, DateTime date, DateTime intime, DateTime outtime, double regular, double overtime, int punchID, PunchTypes punchType = PunchTypes.REGULAR)
         {
             lineNum = lineNumber;
             Date = date;
             In = intime;
             Out = outtime;

             Entry = Out.Subtract(In);

             Regular = regular;
             Overtime = overtime;

             PunchID = punchID;
             PunchType = punchType;

             generateText();

         }

         public TimeCardView(int lineNumber, DateTime date, DateTime intime, DateTime outtime, int punchID, PunchTypes punchType = PunchTypes.REGULAR)
         {
             lineNum = lineNumber;
             Date = date;
             In = intime;
             Out = outtime;

             Regular = 0;
             Overtime = 0;

             Entry = Out.Subtract(In);

             PunchType = punchType;
             PunchID = punchID;

             generateText();
         }

         public TimeCardView()
         {
         }

         public void updateEntry()
         {
             Entry = Out.Subtract(In);
         }

     }
}