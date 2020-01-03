using System;

using Nest;

namespace RigAPI.Wrappers
{
    public sealed class Elastic
    {
        public readonly ElasticClient client;

        public Elastic (string connectionString, string index)
        {
            var uri = new Uri($"http://{connectionString}/");

            var settings = new ConnectionSettings(uri)
                .DefaultIndex(index);

            client = new ElasticClient(settings);
        }
    }
}