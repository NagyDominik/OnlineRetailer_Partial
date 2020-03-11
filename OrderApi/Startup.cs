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

#if DEBUG
        private readonly Uri productServiceUri = new Uri("https://localhost:5004/products/");
        private readonly Uri customerServiceUri = new Uri("https://localhost:5001/customers/");
#else
        private readonly Uri productServiceUri = new Uri("http://productapi/products/");
        private readonly Uri customerServiceUri = new Uri("http://customerapi/customers/");
#endif
        private readonly string cloudAMQPconnectionString = "host=hawk.rmq.cloudamqp.com;virtualHost=lupcpmxk;username=lupcpmxk;password=V50BilRpuuPrQ33ZeRKj0Flq5XAGG0sb";

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

            services.AddSingleton<IServiceGateway<ProductDTO>>(new ProductServiceGateway(productServiceUri));
            services.AddSingleton<IServiceGateway<CustomerDTO>>(new CustomerServiceGateway(customerServiceUri));

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
