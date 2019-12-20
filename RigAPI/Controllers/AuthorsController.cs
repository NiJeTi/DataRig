using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Npgsql;

using RigAPI.Models.Authors;

namespace RigAPI.Controllers
{
    [ApiController]
    [Route("authors")]
    public sealed class AuthorsController : ControllerBase
    {
        private NpgsqlConnection connection;

        public AuthorsController(NpgsqlConnection connection) => this.connection = connection;

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetAuthor(int id)
        {
            throw new NotImplementedException();
        }

        [HttpPost("register")]
        [ProducesResponseType(200)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> RegisterAuthor([FromBody] NewAuthor author)
        {
            throw new NotImplementedException();
        }
    }
}