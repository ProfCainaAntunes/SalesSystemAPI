using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackEndAPI.Data;
using BackEndAPI.Models;

namespace BackEndAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly BackEndAPIContext _context;

        public ClientsController(BackEndAPIContext context)
        {
            _context = context;
        }

        // GET: api/Clients
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetClient()
        {
            return await _context.Client.ToListAsync();
        }

        // GET: api/Clients/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Client>> GetClient(int id)
        {
            var client = await _context.Client.FindAsync(id);

            if (client == null)
            {
                return NotFound();
            }

            return client;
        }

        // GET: api/Clients/bySellerId/2
        [HttpGet("bySellerId/{sellerId}")]
        public async Task<ActionResult<IEnumerable<Client>>> GetClientbySellerId(int sellerId)
        {
            List<Client> clients = await _context.Client
                                    .Where(o => o.SellerId == sellerId)
                                    .ToListAsync();

            if (clients.Count == 0)
                return NotFound();

            return clients;
        }

        // GET: api/Clients/byName/5
        [HttpGet("byName/{name}")]
        public async Task<ActionResult<IEnumerable<Client>>> GetClientbyName(string name)
        {
            List<Client> clients = await _context.Client
                        .Where(o =>o.Name.ToLower().Contains(name.ToLower()))
                        .ToListAsync();

            if (clients.Count == 0)
                return NotFound();

            return clients;
        }

        // PUT: api/Clients/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutClient(int id, Client client)
        {
            Seller? seller = await _context.Seller.FindAsync(client.SellerId);

            if(seller == null)
            {
                return BadRequest("SellerId does not exist.");
            }

            var clientDb = await _context.Client.FindAsync(id);

            if(clientDb == null)
            {
                return BadRequest("Client does not exists.");
            }

            clientDb.SellerId = client.SellerId;
            clientDb.Name = client.Name;
            clientDb.Email = client.Email;
            clientDb.Address = client.Address;
            clientDb.Phone = client.Phone;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("Concurrency conflict occurred while updating the client.");
            }

            return NoContent();
        }

        // POST: api/Clients
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Client>> PostClient(Client client)
        {   
            Seller? seller = await _context.Seller.FindAsync(client.SellerId);

            if(seller == null)
            {
                return BadRequest("SellerId does not exist.");
            }

            _context.Client.Add(client);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetClient", new { id = client.Id }, client);
        }

        // DELETE: api/Clients/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var client = await _context.Client.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            _context.Client.Remove(client);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ClientExists(int id)
        {
            return _context.Client.Any(e => e.Id == id);
        }
    }
}
