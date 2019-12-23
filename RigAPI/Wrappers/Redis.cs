using StackExchange.Redis;

namespace RigAPI.Wrappers
{
    public sealed class Redis
    {
        private object asyncState;

        public readonly ConnectionMultiplexer connection;
        public readonly IDatabase             database;

        public Redis (string connectionString, int databaseNumber)
        {
            asyncState = new object();
            
            connection = ConnectionMultiplexer.Connect(connectionString);
            database   = connection.GetDatabase(databaseNumber, asyncState);
        }
    }
}