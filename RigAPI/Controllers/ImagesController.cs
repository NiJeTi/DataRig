using System.IO;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace RigAPI.Controllers
{
    [ApiController]
    [Route("images")]
    public sealed class ImagesController : ControllerBase
    {
        private GridFSBucket gridFS;

        public ImagesController(IMongoDatabase database) => gridFS = new GridFSBucket(database);

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetImage(string id)
        {
            byte[] imageBytes;

            try
            {
                imageBytes = await gridFS.DownloadAsBytesAsync(ObjectId.Parse(id));
            }
            catch (GridFSFileNotFoundException e)
            {
                return NotFound(e.Message);
            }

            return Ok(File(imageBytes, "image/jpeg"));
        }

        [HttpPut("upload")]
        [ProducesResponseType(typeof(ObjectId), 200)]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            ObjectId newImageId;

            await using (var stream = new MemoryStream())
            {
                await image.CopyToAsync(stream);

                newImageId = await gridFS.UploadFromBytesAsync(image.FileName, stream.ToArray());
            }

            return Ok(newImageId);
        }
    }
}