using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Npgsql;

using RigAPI.Models;
using RigAPI.Wrappers;

namespace RigAPI.Controllers
{
    [ApiController]
    [Route("authors")]
    public sealed class AuthorsController : ControllerBase
    {
        private Postgres postgres;

        public AuthorsController(Postgres postgres) => this.postgres = postgres;

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Author), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetAuthor(int id)
        {
            Author author = default;

            await using (var command = new NpgsqlCommand("SELECT * FROM authors " +
                                                         $"WHERE authors.id = {id}", postgres.connection))
            {
                await using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    author = new Author
                    {
                        ID   = reader[0] as int?,
                        Name = reader[1] as string,
                        Bio  = reader[2] as string
                    };
                }

                await reader.CloseAsync();
            }

            if (author is null)
                return NotFound();

            return Ok(author);
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(int), 200)]
        public async Task<IActionResult> RegisterAuthor([FromBody] Author author)
        {
            int authorID;

            await using (var command =
                new NpgsqlCommand($"SELECT * FROM add_author('{author.Name}', '{author.Bio}')", postgres.connection))
            {
                await using var reader = await command.ExecuteReaderAsync();
                await reader.ReadAsync();

                authorID = (int) reader[0];

                await reader.CloseAsync();
            }

            return Ok(authorID);
        }
    }
}