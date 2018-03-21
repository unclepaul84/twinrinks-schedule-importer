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
                trEvents.FilterTeamEvents(TwinRinksTeamLevel.Squirt, 1).WriteTeamSnapImportFile(sw);

                Debug.Write(sb.ToString());

            }

        }




    }
}
