using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MongoDB.Driver;

using Npgsql;

namespace RigAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();

            string postgresCS = $"Host={Configuration["Postgres:Host"]};" +
                                $"Port={Configuration["Postgres:Port"]};" +
                                $"Username={Configuration["Postgres:Username"]};" +
                                $"Password={Configuration["Postgres:Password"]};" +
                                $"Database={Configuration["Postgres:Database"]}";

            services.AddSingleton<NpgsqlConnection>(new NpgsqlConnection(postgresCS));

            string mongoCS =
                $"mongodb://{Configuration["Mongo:Username"]}:{Configuration["Mongo:Password"]}@{Configuration["Mongo:Host"]}:{Configuration["Mongo:Port"]}";

            services.AddSingleton<IMongoDatabase>(
                new MongoClient(mongoCS).GetDatabase(Configuration["Mongo:Database"]));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}