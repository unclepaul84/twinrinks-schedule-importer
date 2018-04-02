using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DP.TwinRinksHelperWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DP.TwinRinksHelperWeb.Pages
{
    public class IndexModel : PageModel
    {
        private readonly TwinRinksScheduleParserService _twinRinksService;

        public IndexModel(TwinRinksScheduleParserService twinRinksService)
        {
            this._twinRinksService = twinRinksService;
        }

        public SelectList Teams { get; private set; }

        public string SelectedTeam { get; set; }
        public IEnumerable<TwinRinksScheduleParser.TwinRinksEvent> Events { get; private set; }
        public void OnGet(string SelectedTeam)
        {
            Teams = new SelectList(new[] { "Select Team" }.Union(_twinRinksService.GetTeamsList()).ToArray());
            this.SelectedTeam = SelectedTeam;
            Events = _twinRinksService.GetEvents(SelectedTeam);
        }


        public IActionResult OnGetExportTeamSnap(string team)
        {

            byte[] file = _twinRinksService.GetTeamSnapScheduleImportFile(team);

            if (file != null)
                return this.File(file, "text/csv", $"{team.Replace(" ", "_")}_TeamSnap_Import.csv");
            else
                return this.RedirectToPage();

        }

        public IActionResult OnGetICalFile(string team)
        {
            byte[] file = _twinRinksService.GetICalFile(team);

            if (file != null)
                return this.File(file, "text/calendar", $"{team.Replace(" ", "_")}.ics");
            else
                return this.RedirectToPage(new { SelectedTeam = team });

        }
    }
}
