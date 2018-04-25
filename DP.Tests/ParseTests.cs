using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HtmlAgilityPack;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Collections.Generic;
using DP.TwinRinksScheduleParser;
using System.Linq;
using System.Text;
using System.IO;

namespace Tests
{

    [TestClass]
    public class ParseTests
    {
        [TestMethod]
        [DeploymentItem("Samples\\Schedule.html")]
        public void TestTeamSnapExport()
        {
            var doc = new HtmlDocument();

            doc.Load("Schedule.html");

            var trEvents = doc.ParseTwinRinksEvents();

            StringBuilder sb = new StringBuilder();

            using (StringWriter sw = new StringWriter(sb))
            {
                trEvents.FilterTeamEvents(TwinRinksTeamLevel.Squirt, 2).WriteTeamSnapImportFile(sw);

                Debug.Write(sb.ToString());

            }

        }


        [TestMethod]
        [DeploymentItem("Samples\\Schedule.html")]
        public void TestParseTeamMonikers()
        {
            var doc = new HtmlDocument();

            doc.Load("Schedule.html");

            var monikers = doc.ParseTwinRinksEvents().GetTeamMonikers();

        }


        [TestMethod]
        [DeploymentItem("Samples\\Schedule.html")]
        public void TestFindConflicts()
        {
            var doc = new HtmlDocument();

            doc.Load("Schedule.html");

            var gamesOnly = doc.ParseTwinRinksEvents().Where(x => x.EventType == TwinRinksEventType.Game).ToArray();
          
          var s1 = gamesOnly.FilterTeamEvents(TwinRinksTeamLevel.Squirt, 1);

          var s2 = gamesOnly.FilterTeamEvents(TwinRinksTeamLevel.Squirt, 2);

          var conflicts =  s1.FindConflictsWith(s2).ToArray();

            Console.WriteLine(JsonConvert.SerializeObject(conflicts));

        }


        [TestMethod]
       public void TestEventJsonRoundTrip()
        {
            var doc = new HtmlDocument();

            doc.Load("Schedule.html");

            var gamesOnly = doc.ParseTwinRinksEvents().Where(x => x.EventType == TwinRinksEventType.Game).ToArray();

            var s1 = gamesOnly.FilterTeamEvents(TwinRinksTeamLevel.Squirt, 1);

            var s2 = gamesOnly.FilterTeamEvents(TwinRinksTeamLevel.Squirt, 2);

            var conflicts = s1.FindConflictsWith(s2).ToArray();

            Console.WriteLine(JsonConvert.SerializeObject(conflicts));

        }
    }
}
