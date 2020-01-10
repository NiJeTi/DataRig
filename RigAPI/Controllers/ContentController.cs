using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using MongoDB.Bson;

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

        public ContentController(Postgres postgres, Mongo mongo, Redis redis, Elastic elastic, Wrappers.Neo4j neo4j)
        {
            this.postgres = postgres;
            this.mongo    = mongo;
            this.redis    = redis;
            this.elastic  = elastic;
            this.neo4j    = neo4j;
        }

        [HttpGet("articles/{id}")]
        [ProducesResponseType(typeof(OutputArticle), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetArticle(int id)
        {
            var article = new OutputArticle();

            string              textReference;
            IEnumerable<string> imageReferences;

            await using (var command =
                new NpgsqlCommand($"SELECT * FROM compile_article({id})", postgres.connection))
            {
                await using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                    return NotFound();

                article.AuthorName = reader[0] as string;
                textReference      = reader[1] as string;
                imageReferences    = reader[2] as IEnumerable<string>;

                await reader.CloseAsync();
            }

            var elkResponse = await elastic.client.GetAsync<ElasticArticleContent>(textReference);
            article.Title = elkResponse.Source.Title;
            article.Text  = elkResponse.Source.Text;

            foreach (string imageID in imageReferences)
                article.Images.Add(await mongo.gridFS.DownloadAsBytesAsync(ObjectId.Parse(imageID)));

            var neo4jReader = await neo4j.session.RunAsync(
                                  "MATCH (a:Article)-[:REFERENCE]->(b:Article) " +
                                  $"WHERE a.ID = {id} " +
                                  "RETURN b.ID");

            var articleReferences = new List<int>();

            while (await neo4jReader.FetchAsync())
                articleReferences.Add(Convert.ToInt32(neo4jReader.Current[0]));

            article.References.AddRange(await GetTitlesByIDs(articleReferences));

            article.Tags = from tag in redis.server.Keys()
                           where redis.database.SetScan(tag, id).Count() != 0
                           select tag.ToString();

            return Ok(article);
        }

        [HttpGet("images/{id}")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetImage(string id)
        {
            byte[] imageBytes;

            try
            {
                imageBytes = await mongo.gridFS.DownloadAsBytesAsync(ObjectId.Parse(id));
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }

            return Ok(File(imageBytes, "image/jpeg"));
        }

        [HttpGet("tags/{tag}")]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetTag(string tag)
        {
            var set = redis.database.SetScan(tag).ToArray();

            if (!set.Any())
                return NotFound();

            return Ok(await GetTitlesByIDs(set.Select(a => int.Parse(a))));
        }

        [HttpGet("articles/find")]
        [ProducesResponseType(typeof(IEnumerable<ElasticArticleContent>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> FindArticle([FromForm] string text)
        {
            text = text.ToLower();
            
            var searchResult =
                await elastic.client.SearchAsync<ElasticArticleContent>(
                    search => search.Query(
                        query => query.Wildcard(
                            match => match
                                     .Field(field => field.Text)
                                     .Value($"*{text}*"))));

            if (searchResult.Documents.Count == 0)
                return NotFound();

            return Ok(searchResult.Documents);
        }

        [HttpPost("articles/post")]
        [ProducesResponseType(typeof(int), 200)]
        public async Task<IActionResult> PostArticle([FromBody] InputArticle article)
        {
            article.Images     = article.Images.Distinct().ToArray();
            article.References = article.References.Distinct();
            article.Tags       = article.Tags.Distinct();

            int articleID;

            await using (var command = new NpgsqlCommand("SELECT * FROM add_article(" +
                                                         "@author_id, " +
                                                         "NULL, " +
                                                         "@images)",
                                                         postgres.connection))
            {
                command.Parameters.Add("@author_id", NpgsqlDbType.Integer).Value                   = article.AuthorID;
                command.Parameters.Add("@images", NpgsqlDbType.Array | NpgsqlDbType.Varchar).Value = article.Images;

                await using var reader = await command.ExecuteReaderAsync();
                await reader.ReadAsync();

                articleID = (int) reader[0];

                await reader.CloseAsync();
            }

            article.Content.ID = articleID;
            var elasticUpload = await elastic.client.IndexDocumentAsync(article.Content);

            await using (var command = new NpgsqlCommand("UPDATE articles " +
                                                         $"SET text_ref = {elasticUpload.Id} " +
                                                         $"WHERE id = {articleID}", postgres.connection))
            {
                await command.ExecuteNonQueryAsync();
            }

            foreach (string tag in article.Tags)
                await redis.database.SetAddAsync(tag, articleID);

            await neo4j.session.RunAsync("CREATE (a:Article) " +
                                         $"SET a.ID = {articleID}");

            foreach (int reference in article.References)
            {
                await neo4j.session.RunAsync(
                    "MATCH (a:Article),(b:Article) " +
                    $"WHERE a.ID = {articleID} AND b.ID = {reference} " +
                    "CREATE (a)-[:REFERENCE]->(b)");
            }

            return Ok(articleID);
        }

        [HttpPut("images/upload")]
        [ProducesResponseType(typeof(ObjectId), 200)]
        [ProducesResponseType(415)]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            if (image.ContentType != "image/jpeg")
                return StatusCode(415);

            await using var stream = new MemoryStream();

            await image.CopyToAsync(stream);

            var newImageID = await mongo.gridFS.UploadFromBytesAsync(image.FileName, stream.ToArray());

            return Ok(newImageID);
        }

        private async Task<IEnumerable<OutputReference>> GetTitlesByIDs(IEnumerable<int> IDs)
        {
            var enumerable = IDs.ToList();
            var titles     = new List<OutputReference>(enumerable.Count);

            foreach (var command in enumerable.Select(articleID => new NpgsqlCommand(
                                                          "SELECT text_ref FROM articles " +
                                                          $"WHERE id = {articleID}", postgres.connection)))
            {
                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var titleResponse = await elastic.client.GetAsync<ElasticArticleContent>(reader[0] as string);

                    titles.Add(new OutputReference
                    {
                        ID    = titleResponse.Source.ID.Value,
                        Title = titleResponse.Source.Title
                    });
                }

                await reader.CloseAsync();
            }

            return titles;
        }
    }
}