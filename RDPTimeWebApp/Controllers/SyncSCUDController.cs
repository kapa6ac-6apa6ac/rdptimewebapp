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
    public class SyncSCUDController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly OrionContext _orion;
        private readonly SigurMainContext _sigur;

        public SyncSCUDController(AppDbContext context, OrionContext orion, SigurMainContext sigur)
        {
            _context = context;
            _orion = orion;
            _sigur = sigur;
        }

        [HttpPost]
        [Route("reset")]
        public async Task<ActionResult> Reset()
        {
            var users = await _context.Users.ToArrayAsync();

            foreach (var user in users)
            {
                user.ScudSlvId = null;
                user.ScudUfaId = null;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        [HttpPost]
        [Route("salavat")]
        public async Task<ActionResult> SyncSalavat()
        {
            var users = await _context.Users.ToArrayAsync();
            var pUsers = await _orion.PList.Select(p => new { Id = p.Id, Name = $"{p.Name.Trim()} {p.FirstName.Trim()} {p.MidName.Trim()}".Trim().ToUpper(), Tab = p.TabNumber }).ToArrayAsync();

            foreach (var user in users)
            {
                user.ScudSlvId = pUsers.Where(u => u.Name == user.Name.Trim().ToUpper()).Select(u => u.Id).LastOrDefault();
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        [HttpPost]
        [Route("ufa")]
        public async Task<ActionResult> SyncUfa()
        {
            var users = await _context.Users.ToArrayAsync();
            //var pUsers = await _orion.PList.Select(p => new { Id = p.Id, Name = $"{p.Name.Trim()} {p.FirstName.Trim()} {p.MidName.Trim()}".Trim().ToUpper(), Tab = p.TabNumber }).ToArrayAsync();
            var pUsers = await _sigur.Personal.Where(p => p.Type == "EMP" && p.Status != "FIRED").Select(p => new { Id = p.Id, Name = p.Name.Trim().ToUpper() }).ToArrayAsync();

            foreach (var user in users)
            {
                user.ScudUfaId = pUsers.Where(u => u.Name == user.Name.Trim().ToUpper()).Select(u => u.Id).LastOrDefault();
                await _context.SaveChangesAsync();
            }

            return Ok();
        }
    }
}
