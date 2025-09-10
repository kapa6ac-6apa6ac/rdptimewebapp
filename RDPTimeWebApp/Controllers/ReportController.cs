using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Report;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RDPTimeWebApp.DbContexts;
using RDPTimeWebApp.Models.Api;
using RDPTimeWebApp.Models.OrionPro;
using RDPTimeWebApp.Services;

namespace RDPTimeWebApp.Controllers
{
    [Route("api/[controller]")]
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

        [HttpGet("{year}/{month}")]
        public async Task<FileContentResult> GetTimeMonth(int year, int month, bool allUsers = false, bool manicTime = false)
        {
            var date = new DateTime(year, month, 1);

            var reportData = new ReportTimeModel { Date = $"{date.Month}.{date.Year}" };
            //var users = await _context.Connections.Where(c => c.Date.Year == year && c.Date.Month == month).Select(c => c.User).Distinct().ToArrayAsync();
            var users = await _context.Users.ToListAsync();
            var logs = _scud.ConvertIdsToLocal(await _scud.GetLogsAllMonth(date), users);
            var timeSCUD = await _scud.GetTimeFromLogs(logs);
            var timeVector = await _vector.GetTimeMonth(date);

            Dictionary<string, long> timeManic = new Dictionary<string, long>();
            if (manicTime)
                timeManic = await _timeManic.GetTimeRange(date, date.AddMonths(1));

            var ignoredComputers = await _context.Computers.Where(c => _ignoredComputers.Contains(c.Name)).Select(c => c.Id).ToArrayAsync();

            foreach (var user in users)
            {
                var TimeRDP = await _context.Connections.Where(c => c.Date.Year == year && c.Date.Month == month && c.UserId == user.Id && !ignoredComputers.Contains(c.ComputerId)).SumAsync(c => c.Time);
                var userLog = timeSCUD.FirstOrDefault(t => t.UserId == user.Id) ?? new ScudLogGrabber.ScudTime();
                var TimeSCUD = userLog.Time;
                var TimeSCUD_R = userLog.RTime;
                //var TimeVector = await _context.VectorTime.Where(v => v.Year == year && v.Month == month && v.UserId == user.Id).Select(v => v.Time * 60).FirstOrDefaultAsync();
                if (TimeRDP > 0 || TimeSCUD > 0 || (timeVector.ContainsKey(user.Login) && timeVector[user.Login] > 0) || allUsers)
                {
                    var r = new ReportTimeModel.WorkTime
                    {
                        User = user.Login,
                        Name = user.Name,
                        //TimeRDP = FormatTime(TimeRDP),
                        //TimeSCUD = FormatTime(TimeSCUD),
                        //TimeVector = FormatTime(TimeVector)
                        TimeRDP = TimeSpan.FromSeconds(TimeRDP),
                        TimeSCUD = TimeSpan.FromSeconds(TimeSCUD),
                        TimeSCUD_R = TimeSpan.FromSeconds(TimeSCUD_R),
                        //TimeVector = TimeSpan.FromSeconds(TimeVector)
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

        [HttpGet("zp/{year}/{month}")]
        public async Task<FileContentResult> GetRepMonth(int year, int month)
        {
            var date = new DateTime(year, month, 1);

            //var users = await _context.Connections.Where(c => c.Date.Year == year && c.Date.Month == month).Select(c => c.User).Distinct().ToArrayAsync();
            var users = await _context.Users.ToListAsync();
            var logs = _scud.ConvertIdsToLocal(await _scud.GetLogsAllMonth(date), users);
            var timeSCUD = await _scud.GetTimeFromLogs(logs);
            var reportData = new List<ZpType>();
            Dictionary<string, long> timeManic = await _timeManic.GetTimeRange(date, date.AddMonths(1));

            var ignoredComputers = await _context.Computers.Where(c => _ignoredComputers.Contains(c.Name)).Select(c => c.Id).ToArrayAsync();

            foreach (var user in users)
            {
                var TimeRDP = await _context.Connections.Where(c => c.Date.Year == year && c.Date.Month == month && c.UserId == user.Id && !ignoredComputers.Contains(c.ComputerId)).SumAsync(c => c.Time);
                var timeLog = timeSCUD.FirstOrDefault(t => t.UserId == user.Id) ?? new ScudLogGrabber.ScudTime();
                var TimeSCUD = timeLog.Time;
                var TimeSCUD_R = timeLog.RTime;
                var zp = new ZpType
                {
                    Name = user.Name.Trim().ToUpper(),
                    TimeRDP = CalcHours(TimeRDP),
                    TimeSCUD = CalcHours((int)TimeSCUD),
                    TimeSCUD_R = CalcHours((int)TimeSCUD_R)
                };
                if (timeManic.ContainsKey(user.Login))
                    zp.TimeManic = CalcHours((int)timeManic[user.Login]);
                reportData.Add(zp);
            }

            var workbook = new ClosedXML.Excel.XLWorkbook(@"./Template/ZpReport.xlsx");
            var ws1 = workbook.Worksheet(1);

            int i = ws1.FirstRowUsed().RowNumber();
            int l = ws1.LastRowUsed().RowNumber();
            for (var row = ws1.Row(i); row.RowNumber() < l; row = ws1.Row(++i))
            {
                var name = row.Cell(6).GetValue<string>().Trim().ToUpper();
                var user = reportData.FirstOrDefault(u => u.Name == name);
                if (user != null)
                {
                    row.Cell(7).SetValue(user.TimeSCUD);
                    row.Cell(8).SetValue(user.TimeRDP);
                    row.Cell(9).SetFormulaR1C1($"=RC[-2]+RC[-1]");
                    row.Cell(10).SetValue(user.TimeManic);
                    row.Cell(11).SetValue(user.TimeSCUD_R + user.TimeRDP);
                }
            }

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{month}.{year}.xlsx");
            }
        }

        [HttpGet("zp/{fromYear}/{fromMonth}/{fromDay}/{toYear}/{toMonth}/{toDay}")]
        public async Task<FileContentResult> GetRepDayRange(int fromYear, int fromMonth, int fromDay, int toYear, int toMonth, int toDay)
        {
            var fromDate = new DateTime(fromYear, fromMonth, fromDay);
            var toDate = new DateTime(toYear, toMonth, toDay);

            //var users = await _context.Connections.Where(c => c.Date.Year == year && c.Date.Month == month).Select(c => c.User).Distinct().ToArrayAsync();
            var users = await _context.Users.ToListAsync();
            var logs = _scud.ConvertIdsToLocal(await _scud.GetLogsAllPeriod(fromDate, toDate), users);
            var timeSCUD = await _scud.GetTimeFromLogs(logs);
            var reportData = new List<ZpType>();
            var timeManic = await _timeManic.GetTimeRange(fromDate, toDate.AddDays(1));

            var ignoredComputers = await _context.Computers.Where(c => _ignoredComputers.Contains(c.Name)).Select(c => c.Id).ToArrayAsync();

            foreach (var user in users)
            {
                var TimeRDP = await _context.Connections.Where(c => c.Date >= fromDate && c.Date <= toDate && c.UserId == user.Id && !ignoredComputers.Contains(c.ComputerId)).SumAsync(c => c.Time);
                var timeLog = timeSCUD.FirstOrDefault(t => t.UserId == user.Id) ?? new ScudLogGrabber.ScudTime();
                var TimeSCUD = timeLog.Time;
                var TimeSCUD_R = timeLog.RTime;
                var zp = new ZpType
                {
                    Name = user.Name.Trim().ToUpper(),
                    TimeRDP = CalcHours(TimeRDP),
                    TimeSCUD = CalcHours((int)TimeSCUD),
                    TimeSCUD_R = CalcHours((int)TimeSCUD_R)
                };
                if (timeManic.ContainsKey(user.Login))
                    zp.TimeManic = CalcHours((int)timeManic[user.Login]);
                reportData.Add(zp);
            }

            var workbook = new ClosedXML.Excel.XLWorkbook(@"./Template/ZpReport.xlsx");
            var ws1 = workbook.Worksheet(1);

            int i = ws1.FirstRowUsed().RowNumber();
            int l = ws1.LastRowUsed().RowNumber();
            for (var row = ws1.Row(i); row.RowNumber() < l; row = ws1.Row(++i))
            {
                var name = row.Cell(6).GetValue<string>().Trim().ToUpper();
                var user = reportData.FirstOrDefault(u => u.Name == name);
                if (user != null)
                {
                    row.Cell(7).SetValue(user.TimeSCUD);
                    row.Cell(8).SetValue(user.TimeRDP);
                    row.Cell(9).SetFormulaR1C1($"=RC[-2]+RC[-1]");
                    row.Cell(10).SetValue(user.TimeManic);
                    row.Cell(11).SetValue(user.TimeSCUD_R + user.TimeRDP);
                }
            }

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fromDay}.{fromMonth}.{fromYear}-{toDay}.{toMonth}.{toYear}.xlsx");
            }
        }

        [HttpGet("zp/{year}/{month}/{day}")]
        public async Task<FileContentResult> GetRepDay(int year, int month, int day)
        {
            var date = new DateTime(year, month, day);

            //var users = await _context.Connections.Where(c => c.Date.Year == year && c.Date.Month == month).Select(c => c.User).Distinct().ToArrayAsync();
            var users = await _context.Users.ToListAsync();
            var logs = _scud.ConvertIdsToLocal(await _scud.GetLogsAllDay(date), users);
            var timeSCUD = await _scud.GetTimeFromLogs(logs);
            var reportData = new List<ZpType>();
            var timeManic = await _timeManic.GetTimeRange(date, date.AddDays(1));

            var ignoredComputers = await _context.Computers.Where(c => _ignoredComputers.Contains(c.Name)).Select(c => c.Id).ToArrayAsync();

            foreach (var user in users)
            {
                var TimeRDP = await _context.Connections.Where(c => c.Date.Year == year && c.Date.Month == month && c.Date.Day == day && c.UserId == user.Id && !ignoredComputers.Contains(c.ComputerId)).SumAsync(c => c.Time);
                var timeLog = timeSCUD.FirstOrDefault(t => t.UserId == user.Id) ?? new ScudLogGrabber.ScudTime();
                var TimeSCUD = timeLog.Time;
                var TimeSCUD_R = timeLog.RTime;
                var zp = new ZpType
                {
                    Name = user.Name.Trim().ToUpper(),
                    TimeRDP = CalcHours(TimeRDP),
                    TimeSCUD = CalcHours((int)TimeSCUD),
                    TimeSCUD_R = CalcHours((int)TimeSCUD_R)
                };
                if (timeManic.ContainsKey(user.Login))
                    zp.TimeManic = CalcHours((int)timeManic[user.Login]);
                reportData.Add(zp);
            }

            var workbook = new ClosedXML.Excel.XLWorkbook(@"./Template/ZpReport.xlsx");
            var ws1 = workbook.Worksheet(1);

            int i = ws1.FirstRowUsed().RowNumber();
            int l = ws1.LastRowUsed().RowNumber();
            for (var row = ws1.Row(i); row.RowNumber() < l; row = ws1.Row(++i))
            {
                var name = row.Cell(6).GetValue<string>().Trim().ToUpper();
                var user = reportData.FirstOrDefault(u => u.Name == name);
                if (user != null)
                {
                    row.Cell(7).SetValue(user.TimeSCUD);
                    row.Cell(8).SetValue(user.TimeRDP);
                    row.Cell(9).SetFormulaR1C1($"=RC[-2]+RC[-1]");
                    row.Cell(10).SetValue(user.TimeManic);
                    row.Cell(11).SetValue(user.TimeSCUD_R + user.TimeRDP);
                }
            }

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{month}.{year}.xlsx");
            }
        }

        private static float CalcHours(int t)
        {
            long h = t / 3600;
            long m = (t - h * 3600) / 60;

            float hf = h;

            if (m >= 30)
                hf += 1f;
            else if (m >= 19)
                hf += .5f;

            return hf;
        }

        private class ZpType
        {
            public string Name { get; set; }
            public float TimeRDP { get; set; }
            public float TimeSCUD { get; set; }
            public float TimeSCUD_R { get; set; }
            public float? TimeManic { get; set; }
        }

        [HttpGet("{year}/{month}/{day}")]
        public async Task<FileContentResult> GetTimeDay(int year, int month, int day, bool allUsers = false, bool manicTime = false)
        {
            var date = new DateTime(year, month, day);

            var reportData = new ReportTimeModel { Date = $"{date.Day}.{date.Month}.{date.Year}" };
            //var users = await _context.Connections.Where(c => c.Date.Year == year && c.Date.Month == month).Select(c => c.User).Distinct().ToArrayAsync();
            var users = await _context.Users.ToListAsync();
            var logs = _scud.ConvertIdsToLocal(await _scud.GetLogsAllDay(date), users);
            var timeSCUD = await _scud.GetTimeFromLogs(logs);
            var timeVector = await _vector.GetTime(date);
            Dictionary<string, long> timeManic = new Dictionary<string, long>();
            if (manicTime)
                timeManic = await _timeManic.GetTimeRange(date, date.AddDays(1));

            var ignoredComputers = await _context.Computers.Where(c => _ignoredComputers.Contains(c.Name)).Select(c => c.Id).ToArrayAsync();

            foreach (var user in users)
            {
                var TimeRDP = await _context.Connections.Where(c => c.Date == date && c.UserId == user.Id && !ignoredComputers.Contains(c.ComputerId)).SumAsync(c => c.Time);
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

        private static string FormatTime(long? time)
        {
            if (!time.HasValue)
                return "-";
            else if (time.Value == -1)
                return "-";

            var t = time.Value;

            long h = t / 3600;
            long m = (t - h * 3600) / 60;
            long s = t - h * 3600 - m * 60;

            return string.Format("{0:00}:{1:00}:{2:00}", h, m, s);
        }

        private static string FormatTime(int time)
        {
            if (time == -1)
                return "-";

            int h = time / 3600;
            int m = (time - h * 3600) / 60;
            int s = time - h * 3600 - m * 60;

            return string.Format("{0:00}:{1:00}:{2:00}", h, m, s);
        }
    }
}
