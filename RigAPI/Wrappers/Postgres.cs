using Npgsql;

namespace RigAPI.Wrappers
{
    public sealed class Postgres
    {
        public readonly NpgsqlConnection connection;

        public Postgres (string connectionString)
        {
            connection = new NpgsqlConnection(connectionString);
            connection.Open();
        }
    }
}