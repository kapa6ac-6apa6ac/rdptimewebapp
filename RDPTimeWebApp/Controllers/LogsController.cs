using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RDPTimeWebApp.DbContexts;
using RDPTimeWebApp.Models.Api;
using RDPTimeWebApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDPTimeWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private ScudLogGrabber _scud;
        private AppDbContext _context;

        public LogsController(AppDbContext context, OrionContext orion, ScudLogGrabber scud)
        {
            _scud = scud;
            _context = context;
        }

        [HttpGet("{id}/{year}/{month}/{day}")]
        public async Task<ActionResult<LogsModel>> GetTimeDay(int id, int year, int month, int day)
        {
            var date = new DateTime(year, month, day);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return NotFound();

            var result = new LogsModel
            {
                User = user.Login,
                Name = user.Name,
                RDPLogs = await _context.Connections
                    .Where(c => c.Date == date && c.UserId == id)
                    .OrderBy(c => c.DateTime)
                    .Select(c => new LogsModel.RDPLog { Time = c.DateTime, Duration = c.Time, Computer = c.Computer.Name })
                    .ToListAsync(),
                SCUDLogs = (await _scud.GetLogsAllDay(date, user)).Select(l => new LogsModel.SCUDLog
                    {
                        Time = l.Time,
                        Door = l.DoorId,
                        Event = (int)l.Event, 
                        Mode = (int)l.Type,
                        City = (int)l.City
                    }).ToList()
            };

            return result;
        }

        [HttpGet("{id}/{year}/{month}")]
        public async Task<ActionResult<LogsModel>> GetTimeMonth(int id, int year, int month)
        {
            var date = new DateTime(year, month, 1);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return NotFound();

            var result = new LogsModel
            {
                User = user.Login,
                Name = user.Name,
                RDPLogs = await _context.Connections
                    .Where(c => c.Date.Year == year && c.Date.Month == month && c.UserId == id)
                    .OrderBy(c => c.DateTime)
                    .Select(c => new LogsModel.RDPLog { Time = c.DateTime, Duration = c.Time, Computer = c.Computer.Name })
                    .ToListAsync(),
                SCUDLogs = (await _scud.GetLogsAllMonth(date, user)).Select(l => new LogsModel.SCUDLog
                {
                    Time = l.Time,
                    Door = l.DoorId,
                    Event = (int)l.Event,
                    Mode = (int)l.Type,
                    City = (int)l.City
                }).ToList()
            };

            return result;
        }
    }
}
