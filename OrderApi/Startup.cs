using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DTOs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderApi.Data;
using OrderApi.Infrastructure;
using OrderApi.Models;

namespace OrderApi
{
    public class Startup
    {
        private readonly Uri productServiceeUri = new Uri("http://productapi/products/");

        private readonly string cloudAMQPconnectionString = "host=hare.rmq.cloudamqp.com;virtualHost=npaprqop;username=npaprqop;password=putyourpasswordhere";


        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // In-memory database:
            services.AddDbContext<OrderApiContext>(opt => opt.UseInMemoryDatabase("OrdersDb"));

            // Register repositories for dependency injection
            services.AddScoped<IRepository<Order>, OrderRepository>();

            // Register database initializer for dependency injection
            services.AddTransient<IDbInitializer, DbInitializer>();

            services.AddSingleton<IServiceGateway<ProductDTO>>(new ProductServiceGateway(productServiceeUri));

            services.AddSingleton<IMessagePublisher>(new MessagePublisher(cloudAMQPconnectionString));

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
                var dbContext = services.GetService<OrderApiContext>();
                var dbInitializer = services.GetService<IDbInitializer>();
                dbInitializer.Initialize(dbContext);
            }

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
