using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HtmlAgilityPack;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Tests
{

    [TestClass]
    public class Expirements
    {
        [TestMethod]
        [DeploymentItem("Samples\\Schedule.html")]
        public void FindTable()
        {
            var doc = new HtmlDocument();
            doc.Load("Schedule.html");

            var rows = doc.DocumentNode.SelectNodes("//td");

            int i = 0;

            TwinRinksParsedScheduleItem currItem = new TwinRinksParsedScheduleItem();

            List<TwinRinksParsedScheduleItem> items = new List<TwinRinksParsedScheduleItem>();

            foreach (var r in rows)
            {
                switch (i)
                {
                    case 0:

                        currItem.Date = r.InnerText.Trim();

                        break;

                    case 1:

                        currItem.Day = r.InnerText.Trim();
                        break;

                    case 2:
                        currItem.Rink = r.InnerText.Trim();

                        break;
                    case 3:

                        currItem.Start = r.InnerText.Trim();
                        break;

                    case 4:
                        currItem.End = r.InnerText.Trim();

                        break;

                    case 5:

                        currItem.Location = r.InnerText.Trim();

                        break;
                    case 6:

                        currItem.Description = r.InnerText.Trim();

                        break;

                    case 7:

                        currItem.Home = r.InnerText.Trim();

                        break;

                    case 8:

                        currItem.Away = r.InnerText.Trim();

                        break;
                }


                i++;

                if (i == 9)
                {
                    i = 0;

                    items.Add(currItem);

                    currItem = new TwinRinksParsedScheduleItem();
                }
            }

            foreach (var tr in items)
            {
                Debug.WriteLine(tr);
            }


        }

        public class TwinRinksParsedScheduleItem
        {
            public string Date { get; set; }
            public string Day { get; set; }
            public string Rink { get; set; }
            public string Start { get; set; }
            public string End { get; set; }
            public string Location { get; set; }
            public string Description { get; set; }
            public string Home { get; set; }
            public string Away { get; set; }

            public override string ToString()
            {
                return JsonConvert.SerializeObject(this);
            }
        }

        public class TwinRinksEvent
        {
            public enum TwinRinksEventType : int
            {
                Game,
                Practice


            }

            public enum TwinRinksRink : int
            {
                Blue,
                Red,
                Away
            }
            public DateTime EventDate { get; set; }
            public TwinRinksEventType EventType { get; set; }
            public TwinRinksRink Rink { get; set; }
            
            public TimeSpan Start { get; set; }
            public string End { get; set; }
            public string Location { get; set; }
            public string Description { get; set; }
            public string Home { get; set; }
            public string Away { get; set; }

        }
    }
}
