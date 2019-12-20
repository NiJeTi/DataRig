namespace RigAPI.Models.Articles
{
    public sealed class NewArticle
    {
        public int      AuthorID { get; set; }
        public string   Text     { get; set; }
        public string[] Images   { get; set; }
        public string[] Tags     { get; set; }
    }
}