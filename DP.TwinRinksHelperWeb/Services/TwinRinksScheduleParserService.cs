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

        internal IEnumerable<TwinRinksEventConflict> GetGameConficts(string selectedTeam1, string selectedTeam2)
        {
            if (!string.IsNullOrWhiteSpace(selectedTeam1) && !string.IsNullOrWhiteSpace(selectedTeam2))
            {
                if (TwinRinksScheduleParserUtils.TryParseTeamLevelAndMoniker(selectedTeam1, out TwinRinksTeamLevel level1, out string moniker1))
                {
                    if (TwinRinksScheduleParserUtils.TryParseTeamLevelAndMoniker(selectedTeam2, out TwinRinksTeamLevel level2, out string moniker2))
                    {

                        var games = this.Events.Where(e => e.EventType == TwinRinksEventType.Game).ToArray();

                        var team1Games = games.FilterTeamEvents(level1, moniker1);
                        var team2Games = games.FilterTeamEvents(level2, moniker2);

                        return team1Games.FindConflictsWith(team2Games);

                    }
                }

            }
            return null;
        }

        private string DownloadSchedulePageContent()
        {
            return new WebClient().DownloadString(ScheduleUrl);
        }

        public List<string> GetTeamsList()
        {
            return _memoryCache.GetOrCreate("TeamsList", (ce) =>
            {
                ce.AbsoluteExpiration = DateTimeOffset.Now.AddHours(1);

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

            });  
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
                return Events.FilterTeamEvents(level, moniker).Where(x=>x.EventDate >= DateTime.Today);
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
