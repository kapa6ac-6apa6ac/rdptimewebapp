using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RDPTimeWebApp.DbContexts;
using RDPTimeWebApp.Models.ApiV2;
using RDPTimeWebApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDPTimeWebApp.Controllers.v2
{
    //[NSwag.Annotations.SwaggerTag("", )]
    [Route("api/v2/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ScudLogGrabber _scud;
        private readonly TimeManicService _manic;
        private readonly VectorService _vector;
        private readonly ILogger<UserController> _logger;

        private readonly string[] _ignoredComputers;

        public UserController(AppDbContext context, ScudLogGrabber scud, TimeManicService manic, VectorService vector, IConfiguration config, ILogger<UserController> logger)
        {
            _context = context;
            _scud = scud;
            _manic = manic;
            _vector = vector;
            _logger = logger;

            _ignoredComputers = config.GetSection("IgnoredComputers").Get<string[]>();
        }

        [Authorize(Roles = "admin, view-all")]
        [HttpGet]
        public async Task<ActionResult> GetUsers()
        {
            return Ok(await _context.Users.ToArrayAsync());
        }

        [Authorize(Roles = "admin")]
        [HttpPut]
        public async Task<ActionResult> ChangeUser([FromBody] Models.UserModel user)
        {
            var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            if (dbUser == null)
                return NotFound();

            dbUser.Login = user.Login;
            dbUser.Name = user.Name;
            dbUser.ScudSlvId = user.ScudSlvId;
            dbUser.ScudUfaId = user.ScudUfaId;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Roles = "admin, view-all")]
        [HttpGet("time")]
        public async Task<ActionResult> GetUsersTime(int id, DateTime? from, DateTime? to, bool manicTime = false)
        {
            from = from ?? DateTime.Now - TimeSpan.FromDays(7);
            from = new DateTime(from.Value.Year, from.Value.Month, from.Value.Day);
            to = to ?? DateTime.Now;
            to = new DateTime(to.Value.Year, to.Value.Month, to.Value.Day);

            var timeVector = await _vector.GetTime(from.Value, to.Value);

            Dictionary<string, long> timeManic = new Dictionary<string, long>();
            if (manicTime)
                timeManic = await _manic.GetTimeRange(from.Value, to.Value);

            //var timeModel = new Models.Api.TimeModel { Date = from.Value };
            var times = new List<UserTime>();
            var users = await _context.Users.ToListAsync();
            var logs = _scud.ConvertIdsToLocal(await _scud.GetLogsAllPeriod(from.Value, to.Value), users);
            var timeSCUD = await _scud.GetTimeFromLogs(logs, true);

            var ignoredComputers = await _context.Computers.Where(c => _ignoredComputers.Contains(c.Name)).Select(c => c.Id).ToArrayAsync();

            foreach (var user in users)
            {
                var log = timeSCUD.FirstOrDefault(l => l.UserId == user.Id);
                times.Add(new UserTime
                {
                    Id = user.Id,
                    Login = user.Login,
                    Name = user.Name,
                    TimeRdp = await _context.Connections.Where(c => c.Date >= from.Value && c.Date <= to.Value.AddDays(1) && c.UserId == user.Id && !ignoredComputers.Contains(c.ComputerId)).SumAsync(c => c.Time),
                    TimeScud = log != null ? log.Time : 0,
                    TimeScudA = log != null ? log.RTime : 0,
                    ScudFrom = log != null ? (log.StartTime.HasValue ? log.StartTime : null) : null,
                    ScudTo = log != null ? (log.EndTime.HasValue ? log.EndTime : null) : null,
                    TimeVector = timeVector.ContainsKey(user.Login) ? timeVector[user.Login] * 60 : 0,
                    TimeManic = manicTime && timeManic.ContainsKey(user.Login) ? timeManic[user.Login] : 0
                });
            }
            //timeModel.TimeData = timeModel.TimeData.Where(t => (t.TimeRDP.HasValue && t.TimeRDP.Value > 0) || (!string.IsNullOrEmpty(t.SCUDStart))).OrderBy(t => t.User).ToList();
            return Ok(times);
        }

        [Authorize(Roles = "admin, view-all")]
        [HttpGet("{id}/time/rdp")]
        public async Task<ActionResult> GetRdpTime(int id, DateTime? from, DateTime? to)
        {
            from = from ?? DateTime.Now - TimeSpan.FromDays(7);
            from = new DateTime(from.Value.Year, from.Value.Month, from.Value.Day);
            to = to ?? DateTime.Now;
            to = new DateTime(to.Value.Year, to.Value.Month, to.Value.Day);

            var user = await GetUser(id);
            return Ok(await _context.Connections
                .Where(c => c.UserId == user.Id && c.Date >= from.Value && c.Date <= to.Value)
                .Select(c => new { c.DateTime, Computer = c.Computer.Name, c.Time, c.IpAddress })
                .ToArrayAsync());
        }

        [Authorize(Roles = "admin, view-all")]
        [HttpGet("{id}/time/scud")]
        public async Task<ActionResult> GetScudTime(int id, DateTime? from, DateTime? to)
        {
            from = from ?? DateTime.Now - TimeSpan.FromDays(7);
            from = new DateTime(from.Value.Year, from.Value.Month, from.Value.Day);
            to = to ?? DateTime.Now;
            to = new DateTime(to.Value.Year, to.Value.Month, to.Value.Day);

            var user = await GetUser(id);
            return Ok((await _scud.GetLogsAllPeriod(from.Value, to.Value, user)).Select(l => new { l.Time, l.DoorId, l.City, l.Type, l.Event }).ToArray());

            //return Ok(await _context.Connections
            //    .Where(c => c.UserId == user.Id && c.Date >= from.Value && c.Date <= to.Value)
            //    .Select(c => new { c.DateTime, Computer = c.Computer.Name, c.Time, c.IpAddress })
            //    .ToArrayAsync());
        }

        [Authorize(Roles = "admin, view-all")]
        [HttpGet("{id}/time")]
        public async Task<ActionResult> GetTime(int id, DateTime? from, DateTime? to)
        {
            from = from ?? DateTime.Now - TimeSpan.FromDays(7);
            from = new DateTime(from.Value.Year, from.Value.Month, from.Value.Day);
            to = to ?? DateTime.Now;
            to = new DateTime(to.Value.Year, to.Value.Month, to.Value.Day);

            var user = await GetUser(id);
            var timeVector = await _vector.GetTimeUser(user, from.Value, to.Value);
            var timeManic = await _manic.GetUserTimePerDay(user, from.Value, to.Value);

            var ignoredComputers = await _context.Computers.Where(c => _ignoredComputers.Contains(c.Name)).Select(c => c.Id).ToArrayAsync();

            List<UserTime> time = new List<UserTime>();
            for (var date = from.Value; date <= to.Value; date = date.AddDays(1))
            {
                var timeRdp = await _context.Connections.Where(c => c.Date == date && c.UserId == user.Id && !ignoredComputers.Contains(c.ComputerId)).SumAsync(c => c.Time);
                var timeScud = (await _scud.GetTimeFromLogs(_scud.ConvertIdsToLocal(await _scud.GetLogsAllDay(date, user), new List<Models.UserModel> { user }), true)).FirstOrDefault();

                time.Add(new UserTime
                {
                    Date = date,
                    TimeRdp = timeRdp,
                    TimeScud = timeScud != null ? timeScud.Time : 0,
                    TimeScudA = timeScud != null ? timeScud.RTime : 0,
                    ScudFrom = timeScud != null ? timeScud.StartTime : null,
                    ScudTo = timeScud != null ? timeScud.EndTime : null,
                    TimeVector = timeVector.ContainsKey(date) ? timeVector[date] * 60 : 0,
                    TimeManic = timeManic.ContainsKey(date) ? timeManic[date] : 0
                });
            }

            return Ok(time);
        }

        private async Task<Models.UserModel> GetUser(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}
