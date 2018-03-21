using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace DP.TwinRinksScheduleParser
{
    public static class TwinRinksScheduleParserUtils
    {

        public static TwinRinksEvent ToEvent(this TwinRinksParsedScheduleItem item)
        {

            TwinRinksEvent tre = new TwinRinksEvent();
            tre.AwayTeamName = item.Away;
            tre.HomeTeamName = item.Home;
            tre.Location = ParseLocation(item);
            tre.Rink = ParseRink(item);
            tre.EventType = ParseEventType(item);
            tre.EventDescription = item.Description;
            tre.EventDate = DateTime.Parse(item.Date);
            tre.EventEnd = DateTime.Parse(item.End + "M").TimeOfDay;
            tre.EventStart = DateTime.Parse(item.Start + "M").TimeOfDay;

            return tre;


        }

        public static IEnumerable<TwinRinksEvent> FilterTeamEvents(this IEnumerable<TwinRinksEvent> me, TwinRinksTeamLevel level, object teamDesignator)
        {
            string teamName = $"{level.ToString().ToUpperInvariant()} {teamDesignator.ToString().ToUpperInvariant()}";
            string allTeamsName = $"ALL {level.ToString().ToUpperInvariant()}S";
            string levelStr = level.ToString().ToUpperInvariant();

            foreach (var e in me)
            {

                if (e.EventType == TwinRinksEventType.Game)
                {
                    if (e.HomeTeamName.Equals(teamName, StringComparison.InvariantCultureIgnoreCase))
                        yield return e;
                    else if (e.AwayTeamName.Equals(teamName, StringComparison.InvariantCultureIgnoreCase))
                        yield return e;
                }
                else // practice
                {
                    var isPowerSkate = e.IsPowerSkatingEvent();

                    if (e.HomeTeamName.Equals(teamName, StringComparison.InvariantCultureIgnoreCase))
                        yield return e;
                    else if (e.AwayTeamName.Equals(teamName, StringComparison.InvariantCultureIgnoreCase))
                        yield return e;
                    else if (e.HomeTeamName.Equals(allTeamsName, StringComparison.InvariantCultureIgnoreCase))
                        yield return e;
                    else if (e.AwayTeamName.Equals(allTeamsName, StringComparison.InvariantCultureIgnoreCase))
                        yield return e;
                    else if (isPowerSkate && (e.HomeTeamName.ToUpperInvariant().Contains(levelStr) || e.AwayTeamName.ToUpperInvariant().Contains(levelStr)))
                        yield return e;

                }


            }

        }

        public static void WriteTeamSnapImportFile(this IEnumerable<TwinRinksEvent> me, TextWriter dest)
        {
            dest.WriteLine("Date,Time,Name,Opponent Name,Opponent Contact Name,Opponent Contact Phone Number,Opponent Contact E-mail Address,Location Name,Location Address,Location URL,Location Details,Home or Away,Uniform,Duration (HH:MM),Arrival Time (Minutes),Extra Label,Notes");

            foreach (var evt in me)
            {
                if (evt.EventType == TwinRinksEventType.Game)
                {
                    var homeOrAway = evt.Rink == TwinRinksRink.Away ? "Away" : "Home";
                    var eventName =$"vs {evt.AwayTeamName}";
                    var extraLabel = evt.Rink == TwinRinksRink.Away ? "" : evt.Rink.ToString() + " Rink";

                    dest.WriteLine($"{evt.EventDate.ToString("MM/dd/yyyy")},{evt.EventStart.ToTeamSnapTime()},{eventName},{evt.AwayTeamName},,,,{evt.Location},,,,{homeOrAway},,01:00,40,{extraLabel},");
                }
                else // practice
                {
                    var extraLabel = evt.Rink == TwinRinksRink.Away ? "" : evt.Rink.ToString() + " Rink";

                    var eventName = evt.IsPowerSkatingEvent() ? "Power Skating" : "Practice";

                    dest.WriteLine($"{evt.EventDate.ToString("MM/dd/yyyy")},{evt.EventStart.ToTeamSnapTime()},{eventName},,,,,{evt.Location},,,,,,01:00,20,{extraLabel},{evt.HomeTeamName} {evt.AwayTeamName}");

                }
            }

            

        }

        public static IEnumerable<TwinRinksEvent> ParseTwinRinksEvents(this HtmlAgilityPack.HtmlDocument me)
        {
            var rows = me.DocumentNode.SelectNodes("//td");

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

            return items.Select(x=>x.ToEvent());
        }
        private static string ToTeamSnapTime(this TimeSpan me)
        {
            DateTime time = DateTime.Today.Add(me);

            return time.ToString("hh:mm tt");
        }

        public static bool IsPowerSkatingEvent(this TwinRinksEvent evt)
        {
            return evt.HomeTeamName.EndsWith(" POW") || evt.HomeTeamName.EndsWith(" P") || evt.HomeTeamName.EndsWith(" POWER") ||
               evt.AwayTeamName.EndsWith(" POW") || evt.AwayTeamName.EndsWith(" P") || evt.AwayTeamName.EndsWith(" POWER");

        }
        private static TwinRinksEventType ParseEventType(TwinRinksParsedScheduleItem item)
        {
            return item.Description.Contains("Game") ? TwinRinksEventType.Game : TwinRinksEventType.Practice;
        }

        private static TwinRinksRink ParseRink(TwinRinksParsedScheduleItem item)
        {
            if (item.Rink.Equals("Blue"))
                return TwinRinksRink.Blue;
            else if (item.Rink.Equals("Red"))
                return TwinRinksRink.Red;
            else
                return TwinRinksRink.Away;
        }

        private static string ParseLocation(TwinRinksParsedScheduleItem parsed)
        {
            if (parsed.Location.StartsWith("AT "))
                return parsed.Location.Replace("AT ", "");
            else
                return parsed.Location;
        }
    }
}
