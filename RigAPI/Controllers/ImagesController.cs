using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using MongoDB.Driver;

namespace RigAPI.Controllers
{
    [ApiController]
    [Route("images")]
    public sealed class ImagesController : ControllerBase
    {
        private IMongoDatabase database;

        public ImagesController(IMongoDatabase database) => this.database = database;

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetImage(string id)
        {
            throw new NotImplementedException();
        }

        [HttpPut("upload")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            throw new NotImplementedException();
        }
    }
}