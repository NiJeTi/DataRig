using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace RigAPI.Wrappers
{
    public sealed class Mongo
    {
        public readonly MongoClient    connection;
        public readonly IMongoDatabase database;
        public readonly GridFSBucket   gridFS;

        public Mongo (string connectionString, string databaseName)
        {
            connection = new MongoClient(connectionString);
            database   = connection.GetDatabase(databaseName);
            gridFS     = new GridFSBucket(database);
        }
    }
}