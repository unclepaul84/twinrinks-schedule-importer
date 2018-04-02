using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DP.TwinRinksScheduleParser;
using System.Text;

namespace DP.TwinRinksHelperWeb.Services
{
    public class TwinRinksScheduleParserService
    {
        private readonly string ScheduleUrl = "http://twinrinks.com/youthhockey/schedule_youth.php";

        private readonly IMemoryCache _memoryCache;

        private IEnumerable<TwinRinksEvent> Events
        {
            get
            {
                return _memoryCache.GetOrCreate("Events", (ce) =>
                {
                    ce.AbsoluteExpiration = DateTimeOffset.Now.AddHours(1);

                    var doc = new HtmlDocument();

                    doc.Load(new StringReader(DownloadSchedulePageContent()));

                    return doc.ParseTwinRinksEvents().ToArray();

                });

            }
        }

        public TwinRinksScheduleParserService(IMemoryCache memoryCache)
        {
            this._memoryCache = memoryCache;
        }
        private string DownloadSchedulePageContent()
        {
            return new WebClient().DownloadString(ScheduleUrl);
        }

        public List<string> GetTeamsList()
        {
            var teamMonikers = Events.GetTeamMonikers();

            List<string> res = new List<string>();

            foreach (var kvp in teamMonikers)
            {

                foreach (var l in kvp.Value)
                {
                    res.Add($"{kvp.Key.ToString().ToUpperInvariant()} {l}");
                }
            }

            return res;
        }

        internal byte[] GetICalFile(string team)
        {
            if (TwinRinksScheduleParserUtils.TryParseTeamLevelAndMoniker(team, out TwinRinksTeamLevel level, out string moniker))
            {
                var events = Events.FilterTeamEvents(level, moniker);
                var calValue = events.WriteICalFileString(team);

                return Encoding.UTF8.GetBytes(calValue);
            }


            return null;
        }

        internal byte[] GetTeamSnapScheduleImportFile(string team)
        {
            if (TwinRinksScheduleParserUtils.TryParseTeamLevelAndMoniker(team, out TwinRinksTeamLevel level, out string moniker))
            {
                var events = Events.FilterTeamEvents(level, moniker);

                StringBuilder sb = new StringBuilder();

                using (var sw = new StringWriter(sb))
                {
                    events.WriteTeamSnapImportFile(sw);
                }

                return ASCIIEncoding.ASCII.GetBytes(sb.ToString());
            }

            return null;

        }

        internal IEnumerable<TwinRinksEvent> GetEvents(string team)
        {
            if (TwinRinksScheduleParserUtils.TryParseTeamLevelAndMoniker(team, out TwinRinksTeamLevel level, out string moniker))
            {
                return Events.FilterTeamEvents(level, moniker);
            }

            return null;
        }
    }

    public static class DependencyInjectionExtentions
    {
        public static void AddTwinRinksScheduleParser(this IServiceCollection services)
        {
            services.AddSingleton<TwinRinksScheduleParserService>();
        }

    }
}
