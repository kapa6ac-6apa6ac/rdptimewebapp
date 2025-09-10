using Microsoft.EntityFrameworkCore;
using RDPTimeWebApp.DbContexts;
using RDPTimeWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDPTimeWebApp.Services
{
    public class CalendarService
    {
        private readonly AppDbContext _context;

        public CalendarService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<DateTime, CalendarDayModel>> GetDaysAsync(DateTime from, DateTime to)
        {
            var dbDays = await _context.Calendar.Where(d => d.Date >= from.Date && d.Date <= to.Date).ToArrayAsync();
            var dict = new Dictionary<DateTime, CalendarDayModel>();

            for (var i = from.Date; i <= to.Date; i = i.AddDays(1))
            {
                var day = dbDays.FirstOrDefault(d => d.Date == i.Date);
                if (day == null)
                {
                    day = new CalendarDayModel
                    {
                        Id = -1,
                        Date = i,
                        Name = "",
                        Type = i.DayOfWeek == DayOfWeek.Sunday || i.DayOfWeek == DayOfWeek.Saturday ? CalendarDayModel.DayTypes.Holiday : CalendarDayModel.DayTypes.Working
                    };
                }
                dict.Add(i, day);
            }

            return dict;
        }

        public async Task<CalendarDayModel[]> GetChangedDaysAsync(DateTime from, DateTime to)
        {
            return await _context.Calendar.Where(d => d.Date >= from && d.Date <= to).ToArrayAsync();
        }

        public async Task SetDay(CalendarDayModel day)
        {
            var d = await _context.Calendar.FirstOrDefaultAsync(d => d.Date == day.Date);
            if (d == null)
            {
                d = new CalendarDayModel
                {
                    Date = day.Date.Date,
                    Name = day.Name,
                    Type = day.Type
                };
                await _context.Calendar.AddAsync(d);
                await _context.SaveChangesAsync();
            }
            else
            {
                d.Date = day.Date.Date;
                d.Name = day.Name;
                d.Type = day.Type;
                
                await _context.SaveChangesAsync();
            }
        }
    }
}
