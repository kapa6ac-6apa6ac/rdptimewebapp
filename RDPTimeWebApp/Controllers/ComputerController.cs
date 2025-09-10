using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RDPTimeWebApp.DbContexts;
using RDPTimeWebApp.Models;

namespace RDPTimeWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComputerController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ComputerController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Computer
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ComputerModel>>> GetComputers()
        {
            return await _context.Computers.ToListAsync();
        }

        // GET: api/Computer/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ComputerModel>> GetComputerModel(int id)
        {
            var computerModel = await _context.Computers.FindAsync(id);

            if (computerModel == null)
            {
                return NotFound();
            }

            return computerModel;
        }

        // PUT: api/Computer/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutComputerModel(int id, ComputerModel computerModel)
        {
            if (id != computerModel.Id)
            {
                return BadRequest();
            }

            _context.Entry(computerModel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ComputerModelExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Computer
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<ComputerModel>> PostComputerModel(ComputerModel computerModel)
        {
            _context.Computers.Add(computerModel);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetComputerModel", new { id = computerModel.Id }, computerModel);
        }

        // DELETE: api/Computer/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ComputerModel>> DeleteComputerModel(int id)
        {
            var computerModel = await _context.Computers.FindAsync(id);
            if (computerModel == null)
            {
                return NotFound();
            }

            _context.Computers.Remove(computerModel);
            await _context.SaveChangesAsync();

            return computerModel;
        }

        private bool ComputerModelExists(int id)
        {
            return _context.Computers.Any(e => e.Id == id);
        }
    }
}
