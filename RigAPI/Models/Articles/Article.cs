namespace RigAPI.Models.Articles
{
    public sealed class Article
    {
        public string   AuthorName         { get; set; }
        public string   Text               { get; set; }
        public object[] Images             { get; set; }
        public string[] Tags               { get; set; }
        public int[]    ReferencedArticles { get; set; }
    }
}