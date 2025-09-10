using ClosedXML.Report;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RDPTimeWebApp.DbContexts;
using RDPTimeWebApp.Models.ApiV2;
using RDPTimeWebApp.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RDPTimeWebApp.Controllers.v2
{
    [Route("api/v2/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ScudLogGrabber _scud;
        private readonly VectorService _vector;
        private readonly TimeManicService _timeManic;

        private readonly string[] _ignoredComputers;

        public ReportController(AppDbContext context, ScudLogGrabber scud, VectorService vector, TimeManicService timeManic, IConfiguration config)
        {
            _context = context;
            _scud = scud;
            _vector = vector;
            _timeManic = timeManic;

            _ignoredComputers = config.GetSection("IgnoredComputers").Get<string[]>();
        }

        [Authorize(Roles = "admin, view-all")]
        [HttpGet("common")]
        public async Task<FileContentResult> GetTimeDay(DateTime from, DateTime to, bool allUsers = false, bool manicTime = false)
        {
            var dateFrom = from.Date;
            var dateTo = to.Date;

            var reportData = new ReportTimeModel { Date = dateFrom == dateTo ? $"{dateFrom.Day}.{dateFrom.Month}.{dateFrom.Year}" : $"{dateFrom.Day}.{dateFrom.Month}.{dateFrom.Year} - {dateTo.Day}.{dateTo.Month}.{dateTo.Year}" };
            //var users = await _context.Connections.Where(c => c.Date.Year == year && c.Date.Month == month).Select(c => c.User).Distinct().ToArrayAsync();
            var users = await _context.Users.ToListAsync();
            var logs = _scud.ConvertIdsToLocal(await _scud.GetLogsAllPeriod(dateFrom, dateTo), users);
            var timeSCUD = await _scud.GetTimeFromLogs(logs);
            var timeVector = await _vector.GetTime(dateFrom, dateTo);
            Dictionary<string, long> timeManic = new Dictionary<string, long>();
            if (manicTime)
                timeManic = await _timeManic.GetTimeRange(dateFrom, dateTo.AddDays(1));

            var ignoredComputers = await _context.Computers.Where(c => _ignoredComputers.Contains(c.Name)).Select(c => c.Id).ToArrayAsync();

            foreach (var user in users)
            {
                var TimeRDP = await _context.Connections.Where(c => c.Date >= dateFrom && c.Date <= dateTo && c.UserId == user.Id && !ignoredComputers.Contains(c.ComputerId)).SumAsync(c => c.Time);
                var userLog = timeSCUD.FirstOrDefault(t => t.UserId == user.Id) ?? new ScudLogGrabber.ScudTime();
                var TimeSCUD = userLog.Time;
                var TimeSCUD_R = userLog.RTime;
                if (TimeRDP > 0 || TimeSCUD > 0 || (timeVector.ContainsKey(user.Login) && timeVector[user.Login] > 0) || allUsers)
                {
                    var r = new ReportTimeModel.WorkTime
                    {
                        User = user.Login,
                        Name = user.Name,
                        //TimeRDP = FormatTime(TimeRDP),
                        //TimeSCUD = FormatTime(TimeSCUD)
                        TimeRDP = TimeSpan.FromSeconds(TimeRDP),
                        TimeSCUD = TimeSpan.FromSeconds(TimeSCUD),
                        TimeSCUD_R = TimeSpan.FromSeconds(TimeSCUD_R),
                        TimeVector = TimeSpan.FromMinutes(timeVector.ContainsKey(user.Login) ? timeVector[user.Login] : 0),
                        ManicTime = null
                    };
                    if (manicTime && timeManic.ContainsKey(user.Login))
                        r.ManicTime = TimeSpan.FromSeconds(timeManic[user.Login]);
                    reportData.TimeData.Add(r);
                }
            }
            reportData.TimeData = reportData.TimeData.OrderBy(t => t.User).ToList();

            return File(GenerateReport(reportData), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Отчет за {reportData.Date}.xlsx");
        }

        protected byte[] GenerateReport(ReportTimeModel report)
        {
            var template = new XLTemplate(@"./Template/Report.xlsx");
            template.AddVariable(report);
            template.Workbook.Properties.Author = "ara.snhpro.ru";
            template.Workbook.Properties.LastModifiedBy = "ara.snhpro.ru";
            template.Workbook.Properties.Created = DateTime.Now;
            template.Workbook.Properties.Modified = DateTime.Now;
            template.Workbook.Properties.Company = "Салаватнефтехимпроект";
            template.Workbook.Properties.Title = $"Отчет по RDP за {report.Date}";
            template.Generate();
            using (var stream = new MemoryStream())
            {
                template.SaveAs(stream);
                return stream.ToArray();
            }
        }
    }
}
