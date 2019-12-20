using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Npgsql;

using RigAPI.Models.Articles;

namespace RigAPI.Controllers
{
    [ApiController]
    [Route("articles")]
    public sealed class ArticlesController : ControllerBase
    {
        private NpgsqlConnection connection;

        public ArticlesController(NpgsqlConnection connection) => this.connection = connection;

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Article), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetArticle(int id)
        {
            throw new NotImplementedException();
        }

        [HttpPost("post")]
        [ProducesResponseType(200)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> PostArticle([FromBody] NewArticle article)
        {
            throw new NotImplementedException();
        }
    }
}