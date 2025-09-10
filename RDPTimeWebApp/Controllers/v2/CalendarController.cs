using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RDPTimeWebApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDPTimeWebApp.Controllers.v2
{
    [Route("api/v2/[controller]")]
    [ApiController]
    public class CalendarController : ControllerBase
    {
        private readonly CalendarService _calendar;

        public CalendarController(CalendarService calendar)
        {
            _calendar = calendar;
        }

        [HttpGet]
        public async Task<IActionResult> GetCalendar(DateTime? from, DateTime? to)
        {
            from = from ?? DateTime.Now - TimeSpan.FromDays(7);
            from = new DateTime(from.Value.Year, from.Value.Month, from.Value.Day);
            to = to ?? DateTime.Now;
            to = new DateTime(to.Value.Year, to.Value.Month, to.Value.Day);

            var days = (await _calendar.GetChangedDaysAsync(from.Value, to.Value)).Select(d => new { d.Date, d.Name, d.Type }).ToArray();

            return Ok(days);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> SetDay(Models.CalendarDayModel day)
        {
            await _calendar.SetDay(day);

            return NoContent();
        }
    }
}
