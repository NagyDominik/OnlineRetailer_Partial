using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductApi.Data;
using ProductApi.Infrastructure;
using ProductApi.Models;

namespace ProductApi
{
    public class Startup
    {
        private IWebHostEnvironment _env { get; set; }
        private IConfiguration _conf { get; }

        private string cloudAMQPConnectionString;
        private string sqlConnectionSrting;

        public Startup(IWebHostEnvironment env)
        {
            _env = env;

            if (_env.IsDevelopment())
            {
                //Write your CloudAMQP connection string here.
                cloudAMQPConnectionString = ""; //REMOVE BEFORE COMMITING TO GITHUB!!
            }
            else if (_env.IsProduction())
            {
                cloudAMQPConnectionString = Environment.GetEnvironmentVariable("CloudAMQP");
                sqlConnectionSrting = Environment.GetEnvironmentVariable("SQLServer");
            }

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            _conf = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (_env.IsDevelopment())
            {
                // In-memory database:
                services.AddDbContext<ProductApiContext>(opt => opt.UseInMemoryDatabase("ProductsDb"));
            }
            else if (_env.IsProduction())
            {
                //SQL database:
                services.AddDbContext<ProductApiContext>(opt => opt.UseSqlServer(sqlConnectionSrting));
            }

            // Register repositories for dependency injection
            services.AddScoped<IRepository<Product>, ProductRepository>();

            // Register database initializer for dependency injection
            services.AddTransient<IDbInitializer, DbInitializer>();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Initialize the database
            using (var scope = app.ApplicationServices.CreateScope())
            {
                // Initialize the database
                var services = scope.ServiceProvider;
                var dbContext = services.GetService<ProductApiContext>();
                var dbInitializer = services.GetService<IDbInitializer>();
                //dbInitializer.Initialize(dbContext);
                dbContext.Database.EnsureCreated();
            }
            Task.Factory.StartNew(() =>
                new MessageListener(app.ApplicationServices, cloudAMQPConnectionString).Start());


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
