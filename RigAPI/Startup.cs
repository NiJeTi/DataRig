using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MongoDB.Driver;

using Npgsql;

using RigAPI.Wrappers;

namespace RigAPI
{
    public sealed class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string postgresCS = string.Format("{0}{1}{2}{3}{4}",
                                              $"Host={Configuration["Postgres:Host"]};",
                                              $"Port={Configuration["Postgres:Port"]};",
                                              $"Username={Configuration["Postgres:Username"]};",
                                              $"Password={Configuration["Postgres:Password"]};",
                                              $"Database={Configuration["Postgres:Database"]}");

            string mongoCS =
                $"mongodb://{Configuration["Mongo:Username"]}:{Configuration["Mongo:Password"]}@{Configuration["Mongo:Host"]}:{Configuration["Mongo:Port"]}";

            string redisCS = $"{Configuration["Redis:Host"]}:{Configuration["Redis:Port"]}";

            services.AddControllers().AddNewtonsoftJson();

            services.AddSingleton<Postgres>(p => new Postgres(postgresCS));
            services.AddSingleton<Mongo>(m => new Mongo(mongoCS, Configuration["Mongo:Database"]));
            services.AddSingleton<Redis>(r => new Redis(redisCS, int.Parse(Configuration["Redis:DatabaseNumber"])));
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