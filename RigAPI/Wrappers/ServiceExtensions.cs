using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RigAPI.Wrappers
{
    public static class ServiceExtensions
    {
        public static void AddPostgres(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionString = string.Format("{0}{1}{2}{3}{4}",
                                                    $"Host={configuration["Postgres:Host"]};",
                                                    $"Port={configuration["Postgres:Port"]};",
                                                    $"Username={configuration["Postgres:Username"]};",
                                                    $"Password={configuration["Postgres:Password"]};",
                                                    $"Database={configuration["Postgres:Database"]}");

            var postgres = new Postgres(connectionString);

            services.AddSingleton<Postgres>(postgres);
        }

        public static void AddMongo(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionString =
                $"mongodb://{configuration["Mongo:Username"]}:{configuration["Mongo:Password"]}@{configuration["Mongo:Host"]}:{configuration["Mongo:Port"]}";

            var mongo = new Mongo(connectionString, configuration["Mongo:Database"]);

            services.AddSingleton<Mongo>(mongo);
        }

        public static void AddRedis(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionString = $"{configuration["Redis:Host"]}:{configuration["Redis:Port"]}";

            var redis = new Redis(connectionString, int.Parse(configuration["Redis:DatabaseNumber"]));

            services.AddSingleton<Redis>(redis);
        }

        public static void AddElastic(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionString = $"{configuration["Elastic:Host"]}:{configuration["Elastic:Port"]}";
            string index            = configuration["Elastic:Index"];

            var elastic = new Elastic(connectionString, index);

            services.AddSingleton<Elastic>(elastic);
        }

        public static void AddNeo4j(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionString = $"{configuration["Neo4j:Host"]}:{configuration["Neo4j:Port"]}";
            string username         = configuration["Neo4j:Username"];
            string password         = configuration["Neo4j:Password"];

            var neo4j = new Neo4j(connectionString, username, password);

            services.AddSingleton<Neo4j>(neo4j);
        }
    }
}