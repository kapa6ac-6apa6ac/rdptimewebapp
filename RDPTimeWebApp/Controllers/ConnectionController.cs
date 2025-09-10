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
    public class ConnectionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ConnectionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ConnectionModels
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConnectionModel>>> GetConnections()
        {
            return await _context.Connections.ToListAsync();
        }

        // GET: api/ConnectionModels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ConnectionModel>> GetConnectionModel(long id)
        {
            var connectionModel = await _context.Connections.FindAsync(id);

            if (connectionModel == null)
            {
                return NotFound();
            }

            return connectionModel;
        }

        // PUT: api/ConnectionModels/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutConnectionModel(long id, ConnectionModel connectionModel)
        {
            if (id != connectionModel.Id)
            {
                return BadRequest();
            }

            _context.Entry(connectionModel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ConnectionModelExists(id))
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

        // POST: api/ConnectionModels
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<ConnectionModel>> PostConnectionModel(ConnectionModel connectionModel)
        {
            _context.Connections.Add(connectionModel);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetConnectionModel", new { id = connectionModel.Id }, connectionModel);
        }

        // DELETE: api/ConnectionModels/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ConnectionModel>> DeleteConnectionModel(long id)
        {
            var connectionModel = await _context.Connections.FindAsync(id);
            if (connectionModel == null)
            {
                return NotFound();
            }

            _context.Connections.Remove(connectionModel);
            await _context.SaveChangesAsync();

            return connectionModel;
        }

        private bool ConnectionModelExists(long id)
        {
            return _context.Connections.Any(e => e.Id == id);
        }
    }
}
