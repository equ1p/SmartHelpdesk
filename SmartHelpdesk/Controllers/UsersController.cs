using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using SmartHelpdesk.Dto;
using SmartHelpdesk.Models;

namespace SmartHelpdesk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IDocumentStore _store;

        public UsersController(IDocumentStore store)
        {
            _store = store;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] UserRole? role)
        {
            using var session = _store.OpenAsyncSession();
            var query = session.Query<User>();

            if (role.HasValue)
                query = query.Where(u => u.Role == role.Value);
            
            var users = await query.ToListAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            using var session = _store.OpenAsyncSession();
            var user = await session.LoadAsync<User>(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            using var session = _store.OpenAsyncSession();

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                Role = request.Role
            };

            await session.StoreAsync(user);
            await session.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateUserRequest request)
        {
            using var session = _store.OpenAsyncSession();
            var user = await session.LoadAsync<User>(id);

            if (user == null)
                return NotFound();

            user.Name = request.Name;
            user.Email = request.Email;
            user.Role = request.Role;

            await session.SaveChangesAsync();
            return Ok(user);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            using var session = _store.OpenAsyncSession();
            var user = await session.LoadAsync<User>(id);

            if (user == null)
                return NotFound();

            session.Delete(user);
            await session.SaveChangesAsync();

            return NoContent();
        }
    }
}
