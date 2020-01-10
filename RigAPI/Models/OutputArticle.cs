using System.Collections.Generic;

namespace RigAPI.Models
{
    public sealed class OutputArticle
    {
        public string                AuthorName { get; set; }
        public string                Title      { get; set; }
        public string                Text       { get; set; }
        public List<byte[]>          Images     { get; }
        public List<OutputReference> References { get; }
        public IEnumerable<string>   Tags       { get; set; }

        public OutputArticle ()
        {
            Images     = new List<byte[]>();
            References = new List<OutputReference>();
        }
    }
}