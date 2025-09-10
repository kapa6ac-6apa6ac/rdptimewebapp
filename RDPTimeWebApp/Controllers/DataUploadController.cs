using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RDPTimeWebApp.DbContexts;

namespace RDPTimeWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataUploadController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DataUploadController(AppDbContext context)
        {
            _context = context;
        }

        private async Task<string> ReadStringData()
        {
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                return await reader.ReadToEndAsync();
            }
        }

        // POST: api/DataUpload
        //[HttpPost]
        //public async Task<ActionResult<string>> Post()
        //{
        //    return await ReadStringData();
        //}
        [HttpPost()]
        public async Task<ActionResult> Post()
        {
            var value = await ReadStringData();

            var values = value.Split("\r\n");
            if (values[0] != "!users")
                return BadRequest();
            int i = 1;

            Dictionary<string, int> users = new Dictionary<string, int>();
            for (; values[i] != "!logs"; i++)
            {
                var userInfo = values[i].Split(';');
                var user = _context.Users.Where(u => u.Login == userInfo[0]).FirstOrDefault();
                if (user == null)
                {
                    user = new Models.UserModel
                    {
                        Login = userInfo[0],
                        Name = userInfo[1]
                    };
                    await _context.Users.AddAsync(user);
                    await _context.SaveChangesAsync();
                }
                users.Add(user.Login, user.Id);
            }
            Dictionary<string, int> computers = new Dictionary<string, int>();
            for (i++; values[i] != "!end"; i++)
            {
                var log = values[i].Split(';');
                var logDate = new DateTime(long.Parse(log[1]));
                var logTime = int.Parse(log[3]);

                if (await _context.Connections.AnyAsync(c => c.DateTime == logDate && c.UserId == users[log[0]] && c.Time == logTime))
                    continue;

                if (!computers.ContainsKey(log[2]))
                {
                    var comp = await _context.Computers.FirstOrDefaultAsync(c => c.Name == log[2]);
                    if (comp == null)
                    {
                        comp = new Models.ComputerModel
                        {
                            Name = log[2]
                        };
                        await _context.Computers.AddAsync(comp);
                        await _context.SaveChangesAsync();
                    }
                    computers.Add(comp.Name, comp.Id);
                }

                _context.Connections.Add(new Models.ConnectionModel
                {
                    UserId = users[log[0]],
                    Date = logDate.Date,
                    DateTime = logDate,
                    Time = logTime,
                    ComputerId = computers[log[2]],
                    IpAddress = log[4]
                });
            }
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
