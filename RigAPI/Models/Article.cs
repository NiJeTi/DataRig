using MongoDB.Bson;

namespace RigAPI.Models
{
    public sealed class Article
    {
        public int?       ID       { get; set; }
        public int        AuthorID { get; set; }
        public string     Text     { get; set; }
        public ObjectId[] Images   { get; set; }
        public string[]   Tags     { get; set; }
    }
}