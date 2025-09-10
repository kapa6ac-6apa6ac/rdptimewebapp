using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RDPTimeWebApp.DbContexts;
using RDPTimeWebApp.Models.ApiV2;
using RDPTimeWebApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDPTimeWebApp.Controllers.v2
{
    [Route("api/v2/[controller]")]
    [ApiController]
    public class MeController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ScudLogGrabber _scud;
        private readonly TimeManicService _manic;
        private readonly VectorService _vector;

        private readonly string[] _ignoredComputers;

        public MeController(AppDbContext context, ScudLogGrabber scud, TimeManicService manic, VectorService vector, IConfiguration config)
        {
            _context = context;
            _scud = scud;
            _manic = manic;
            _vector = vector;

            _ignoredComputers = config.GetSection("IgnoredComputers").Get<string[]>();
        }

        [Authorize]
        [HttpGet("info")]
        public async Task<ActionResult> GetInfo()
        {
            return Ok(await GetUser());
        }

        [Authorize]
        [HttpGet("time/rdp")]
        public async Task<ActionResult> GetRdpTime(DateTime? from, DateTime? to)
        {
            from = from ?? DateTime.Now - TimeSpan.FromDays(7);
            from = new DateTime(from.Value.Year, from.Value.Month, from.Value.Day);
            to = to ?? DateTime.Now;
            to = new DateTime(to.Value.Year, to.Value.Month, to.Value.Day);

            var user = await GetUser();
            return Ok(await _context.Connections
                .Where(c => c.UserId == user.Id && c.Date >= from.Value && c.Date <= to.Value)
                .Select(c => new { c.DateTime, Computer = c.Computer.Name, c.Time, c.IpAddress })
                .ToArrayAsync());
        }

        [Authorize]
        [HttpGet("time/scud")]
        public async Task<ActionResult> GetScudTime(DateTime? from, DateTime? to)
        {
            from = from ?? DateTime.Now - TimeSpan.FromDays(7);
            from = new DateTime(from.Value.Year, from.Value.Month, from.Value.Day);
            to = to ?? DateTime.Now;
            to = new DateTime(to.Value.Year, to.Value.Month, to.Value.Day);

            var user = await GetUser();
            return Ok((await _scud.GetLogsAllPeriod(from.Value, to.Value, user)).Select(l => new { l.Time, l.DoorId, l.City, l.Type, l.Event }).ToArray());

            //return Ok(await _context.Connections
            //    .Where(c => c.UserId == user.Id && c.Date >= from.Value && c.Date <= to.Value)
            //    .Select(c => new { c.DateTime, Computer = c.Computer.Name, c.Time, c.IpAddress })
            //    .ToArrayAsync());
        }

        [Authorize]
        [HttpGet("time")]
        public async Task<ActionResult> GetTime(DateTime? from, DateTime? to)
        {
            from = from ?? DateTime.Now - TimeSpan.FromDays(7);
            from = new DateTime(from.Value.Year, from.Value.Month, from.Value.Day);
            to = to ?? DateTime.Now;
            to = new DateTime(to.Value.Year, to.Value.Month, to.Value.Day);

            var user = await GetUser();
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

        private async Task<Models.UserModel> GetUser()
        {
            string email = User.FindFirst(System.Security.Claims.ClaimTypes.Email).Value;
            string login = User.FindFirst("preferred_username").Value;
            if (email.EndsWith("@snhpro.ru"))
            {
                login = "SNHPRO\\" + login;
            }
            else if (email.EndsWith("@ufa.snhpro.ru"))
            {
                login = "UFA\\" + login;
            }

            return await _context.Users.FirstOrDefaultAsync(u => u.Login == login);
        }
    }
}
