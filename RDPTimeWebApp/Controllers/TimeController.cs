using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RDPTimeWebApp.DbContexts;
using RDPTimeWebApp.Models.Api;
using RDPTimeWebApp.Models.OrionPro;
using RDPTimeWebApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDPTimeWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimeController
    {
        private readonly AppDbContext _context;
        private readonly ScudLogGrabber _scud;
        private readonly VectorService _vector;
        private readonly TimeManicService _timeManic;

        public TimeController(AppDbContext context, ScudLogGrabber scud, VectorService vector, TimeManicService timeManic, IConfiguration configuration)
        {
            _context = context;
            _scud = scud;
            _vector = vector;
            _timeManic = timeManic;
        }

        [HttpGet("{year}/{month}")]
        public async Task<ActionResult<TimeModel>> GetTimeMonth(int year, int month, bool timemanic = false)
        {
            var date = new DateTime(year, month, 1);

            var timeModel = new TimeModel { Date = date };
            var users = await _context.Users.ToListAsync();
            var logs = _scud.ConvertIdsToLocal(await _scud.GetLogsAllMonth(date), users);
            var timeSCUD = await _scud.GetTimeFromLogs(logs);
            var timeVector = await _vector.GetTimeMonth(date);
            Dictionary<string, long> timeManic = new Dictionary<string, long>();
            if (timemanic)
                timeManic = await _timeManic.GetTimeRange(date, date.AddMonths(1));

            foreach (var user in users)
            {
                var log = timeSCUD.FirstOrDefault(l => l.UserId == user.Id);
                timeModel.TimeData.Add(new TimeModel.WorkTime
                {
                    Id = user.Id,
                    User = user.Login,
                    Name = user.Name,
                    TimeRDP = await _context.Connections.Where(c => c.Date.Year == year && c.Date.Month == month && c.UserId == user.Id).SumAsync(c => c.Time),
                    TimeSCUD = log != null ? log.Time : 0,
                    TimeSCUD_R = log != null ? log.RTime : 0,
                    TimeVector = timeVector.ContainsKey(user.Login) ? timeVector[user.Login] * 60 : 0,
                    TimeManic = timemanic ? (timeManic.ContainsKey(user.Login) ? timeManic[user.Login] : 0) : 0
                });
            }
            timeModel.TimeData = timeModel.TimeData.Where(t => (t.TimeRDP.HasValue && t.TimeRDP.Value > 0) || (t.TimeSCUD.HasValue && t.TimeSCUD.Value > 0) || (t.TimeVector.HasValue && t.TimeVector.Value > 0)).OrderBy(t => t.User).ToList();
            return timeModel;
        }

        [HttpGet("{year}/{month}/{day}")]
        public async Task<ActionResult<TimeModel>> GetTimeDay(int year, int month, int day, bool timemanic = false)
        {
            var date = new DateTime(year, month, day);

            var timeVector = await _vector.GetTime(date);

            var timeModel = new TimeModel { Date = date };
            var users = await _context.Users.ToListAsync();
            var logs = _scud.ConvertIdsToLocal(await _scud.GetLogsAllDay(date), users);
            var timeSCUD = await _scud.GetTimeFromLogs(logs, true);
            Dictionary<string, long> timeManic = new Dictionary<string, long>();
            if (timemanic)
                timeManic = await _timeManic.GetTimeRange(date, date.AddDays(1));

            foreach (var user in users)
            {
                var log = timeSCUD.FirstOrDefault(l => l.UserId == user.Id);
                timeModel.TimeData.Add(new TimeModel.WorkTime
                {
                    Id = user.Id,
                    User = user.Login,
                    Name = user.Name,
                    TimeRDP = await _context.Connections.Where(c => c.Date == date && c.UserId == user.Id).SumAsync(c => c.Time),
                    TimeSCUD = log != null ? log.Time : 0,
                    TimeSCUD_R = log != null ? log.RTime : 0,
                    SCUDStart = log != null ? (log.StartTime.HasValue ? log.StartTime.Value.ToString("HH:mm:ss") : null) : null,
                    SCUDEnd = log != null ? (log.EndTime.HasValue ? log.EndTime.Value.ToString("HH:mm:ss") : null) : null,
                    TimeVector = timeVector.ContainsKey(user.Login) ? timeVector[user.Login] * 60 : 0,
                    TimeManic = timemanic ? (timeManic.ContainsKey(user.Login) ? timeManic[user.Login] : 0) : 0
                });
            }
            timeModel.TimeData = timeModel.TimeData.Where(t => (t.TimeRDP.HasValue && t.TimeRDP.Value > 0) || (!string.IsNullOrEmpty(t.SCUDStart))).OrderBy(t => t.User).ToList();
            return timeModel;
        }

        [HttpGet("{year}/{month}/{day}/{year2}/{month2}/{day2}")]
        public async Task<ActionResult<TimeModel>> GetTimeCustom(int year, int month, int day, int year2, int month2, int day2)
        {
            var date = new DateTime(year, month, day);
            var date2 = new DateTime(year2, month2, day2);

            var timeVector = await _vector.GetTime(date, date2);

            var timeModel = new TimeModel { Date = date };
            var users = await _context.Users.ToListAsync();
            var logs = _scud.ConvertIdsToLocal(await _scud.GetLogsAllPeriod(date, date2), users);
            var timeSCUD = await _scud.GetTimeFromLogs(logs, true);

            foreach (var user in users)
            {
                var log = timeSCUD.FirstOrDefault(l => l.UserId == user.Id);
                timeModel.TimeData.Add(new TimeModel.WorkTime
                {
                    Id = user.Id,
                    User = user.Login,
                    Name = user.Name,
                    TimeRDP = await _context.Connections.Where(c => c.Date >= date && c.Date <= date2 && c.UserId == user.Id).SumAsync(c => c.Time),
                    TimeSCUD = log != null ? log.Time : 0,
                    TimeSCUD_R = log != null ? log.RTime : 0,
                    SCUDStart = log != null ? (log.StartTime.HasValue ? log.StartTime.Value.ToString("HH:mm:ss") : null) : null,
                    SCUDEnd = log != null ? (log.EndTime.HasValue ? log.EndTime.Value.ToString("HH:mm:ss") : null) : null,
                    TimeVector = timeVector.ContainsKey(user.Login) ? timeVector[user.Login] * 60 : 0
                });
            }
            timeModel.TimeData = timeModel.TimeData.Where(t => (t.TimeRDP.HasValue && t.TimeRDP.Value > 0) || (!string.IsNullOrEmpty(t.SCUDStart))).OrderBy(t => t.User).ToList();
            return timeModel;
        }
    }
}
