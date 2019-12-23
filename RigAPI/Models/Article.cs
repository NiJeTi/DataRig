namespace RigAPI.Models
{
    public sealed class Article
    {
        public int?     ID         { get; set; }
        public int      AuthorID   { get; set; }
        public string   Text       { get; set; }
        public string[] Images     { get; set; }
        public int[]    References { get; set; }
        public string[] Tags       { get; set; }
    }
}