using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RDPTimeWebApp.DbContexts;
using RDPTimeWebApp.Models.Api;

namespace RDPTimeWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VectorDataUploadController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VectorDataUploadController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("{year}/{month}")]
        public async Task<ActionResult> Post(ushort year, byte month, IFormFile file)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var workbook = new XLWorkbook(file.OpenReadStream());
            var ws1 = workbook.Worksheet(1);

            int i = 2;
            for (var row = ws1.Row(i); !row.IsEmpty(); row = ws1.Row(++i))
            {
                var name = row.Cell(1).GetValue<string>();
                var time = row.Cell(3).GetValue<int>();

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Name.Trim().ToUpper() == name.Trim().ToUpper());
                if (user == null)
                    continue;

                var vectorTime = await _context.VectorTime.FirstOrDefaultAsync(v => v.UserId == user.Id && v.Year == year && v.Month == month);
                if (vectorTime != null)
                {
                    vectorTime.Time = time;
                }
                else
                {
                    vectorTime = new Models.VectorTimeModel { Year = year, Month = month, UserId = user.Id, Time = time };
                    await _context.VectorTime.AddAsync(vectorTime);
                }
                await _context.SaveChangesAsync();
            }

            return Ok();
        }
    }
}
