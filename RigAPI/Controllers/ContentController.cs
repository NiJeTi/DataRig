using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using MongoDB.Bson;
using MongoDB.Driver.GridFS;

using Npgsql;

using NpgsqlTypes;

using RigAPI.Models;
using RigAPI.Wrappers;

namespace RigAPI.Controllers
{
    [ApiController]
    public sealed class ContentController : ControllerBase
    {
        private Postgres       postgres;
        private Mongo          mongo;
        private Redis          redis;
        private Elastic        elastic;
        private Wrappers.Neo4j neo4j;

        private GridFSBucket gridFS;

        public ContentController(Postgres postgres, Mongo mongo, Redis redis, Elastic elastic, Wrappers.Neo4j neo4j)
        {
            this.postgres = postgres;
            this.mongo    = mongo;
            this.redis    = redis;
            this.elastic  = elastic;
            this.neo4j    = neo4j;

            gridFS = this.mongo.gridFS;
        }

        [HttpGet("articles/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetArticle(int id)
        {
            throw new NotImplementedException();
        }

        [HttpGet("images/{id}")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetImage(string id)
        {
            byte[] imageBytes;

            try
            {
                imageBytes = await gridFS.DownloadAsBytesAsync(ObjectId.Parse(id));
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }

            return Ok(File(imageBytes, "image/jpeg"));
        }

        [HttpGet("tags/{tag}")]
        [ProducesResponseType(typeof(int[]), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetTag(string tag)
        {
            string value = await redis.database.StringGetAsync(tag);

            if (value is null)
                return NotFound();

            var articles = value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                .Select(int.Parse)
                                .ToArray();

            return Ok(articles);
        }

        [HttpPost("articles/post")]
        [ProducesResponseType(typeof(int), 200)]
        public async Task<IActionResult> PostArticle([FromBody] Article article)
        {
            var elasticResponse = await elastic.client.IndexDocumentAsync(new { text = article.Text });

            int articleID;

            await using (var command = new NpgsqlCommand("SELECT * FROM add_article(" +
                                                         "@author_id," +
                                                         "@text_ref," +
                                                         "@images)",
                                                         postgres.connection))
            {
                command.Parameters.Add("@author_id", NpgsqlDbType.Integer).Value                   = article.AuthorID;
                command.Parameters.Add("@text_ref", NpgsqlDbType.Varchar).Value                    = elasticResponse.Id;
                command.Parameters.Add("@images", NpgsqlDbType.Array | NpgsqlDbType.Varchar).Value = article.Images;

                await using var reader = await command.ExecuteReaderAsync();
                await reader.ReadAsync();

                articleID = (int) reader[0];

                await reader.CloseAsync();
            }

            foreach (string tag in article.Tags)
                await AddTag(tag, articleID);

            // TODO: Throw references to Neo4j

            return Ok(articleID);
        }

        [HttpPut("images/upload")]
        [ProducesResponseType(typeof(ObjectId), 200)]
        [ProducesResponseType(415)]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            // TODO: Save image info to another collection

            if (image.ContentType != "image/jpeg")
                return StatusCode(415);

            ObjectId newImageID;

            await using (var stream = new MemoryStream())
            {
                await image.CopyToAsync(stream);

                newImageID = await gridFS.UploadFromBytesAsync(image.FileName, stream.ToArray());
            }

            return Ok(newImageID);
        }

        private async Task AddTag(string tag, int articleID)
        {
            string existing = await redis.database.StringGetAsync(tag);

            await redis.database.StringSetAsync(tag, $"{existing}{articleID} ");
        }
    }
}