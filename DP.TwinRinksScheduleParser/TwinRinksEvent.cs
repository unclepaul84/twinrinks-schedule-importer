using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DP.TwinRinksScheduleParser
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

    public enum TwinRinksTeamLevel : int
    {
        Mite,
        Squirt,
        PeeWee,
        Bantam,

    }
    public class TwinRinksEvent
    { 
        public DateTime EventDate { get; set; }
        public TwinRinksEventType EventType { get; set; }
        public TwinRinksRink Rink { get; set; }
        public TimeSpan EventStart { get; set; }
        public TimeSpan EventEnd { get; set; }
        public string Location { get; set; }
        public string EventDescription { get; set; }
        public string HomeTeamName { get; set; }
        public string AwayTeamName { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
