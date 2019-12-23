using System;

using Neo4j.Driver;

namespace RigAPI.Wrappers
{
    public sealed class Neo4j : IDisposable
    {
        public readonly IDriver       driver;
        public readonly IAsyncSession session;

        public Neo4j (string connectionString, string username, string password)
        {
            var uri = new Uri($"bolt://{connectionString}/");

            driver = GraphDatabase.Driver(uri, AuthTokens.Basic(username, password));

            session = driver.AsyncSession();
        }

        public void Dispose()
        {
            session.CloseAsync();

            if (driver == null)
                return;

            driver.CloseAsync();
            driver.Dispose();
        }
    }
}