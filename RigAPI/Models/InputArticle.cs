using System.Collections.Generic;

namespace RigAPI.Models
{
    public sealed class InputArticle
    {
        public int                   AuthorID   { get; set; }
        public ElasticArticleContent Content    { get; set; }
        public IEnumerable<string>   Images     { get; set; }
        public IEnumerable<int>      References { get; set; }
        public IEnumerable<string>   Tags       { get; set; }
    }
}