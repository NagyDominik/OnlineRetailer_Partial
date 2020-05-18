using EasyNetQ;
using System;
using System.Threading;
using DTOs;
using Microsoft.Extensions.DependencyInjection;
using ProductApi.Data;
using ProductApi.Models;

namespace ProductApi.Infrastructure
{
    public class MessageListener
    {
        private IServiceProvider provider;
        string connectionString;

        public MessageListener(IServiceProvider provider, string connectionString)
        {
            this.provider = provider;
            this.connectionString = connectionString;
        }

        public void Start()
        {
            using (var bus = RabbitHutch.CreateBus(connectionString))
            {
                bus.Subscribe<OrderStatusChangedMessage>("productApiCompleted", HandleOrderCompleted,
                    x => x = x.WithTopic("completed"));
                
                bus.Subscribe<OrderStatusChangedMessage>("productApiCancelled", HandleOrderCancelled,
                    x => x = x.WithTopic("cancelled"));

                bus.Subscribe<OrderStatusChangedMessage>("productApiShipped", HandleOrderShipped,
                    x => x = x.WithTopic("shipped"));

                lock (this)
                {
                    Monitor.Wait(this);
                }
            }
        }
        private void HandleOrderCompleted(OrderStatusChangedMessage message)
        {
            // A service scope is created to get an instance of the product repository.
            // When the service scope is disposed, the product repository instance will
            // also be disposed.
            using (var scope = provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var productRepos = services.GetService<IRepository<Product>>();

                // Reserve items of ordered product (should be a single transaction).
                // Beware that this operation is not idempotent.
                foreach (var orderLine in message.OrderLines)
                {
                    var product = productRepos.Get(orderLine.ProductId);
                    product.ItemsReserved += orderLine.Quantity;
                    productRepos.Edit(product);
                }
            }
        }

        private void HandleOrderCancelled(OrderStatusChangedMessage message)
        {
            using (var scope = provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var productRepos = services.GetService<IRepository<Product>>();

                foreach (var orderLine in message.OrderLines)
                {
                    var product = productRepos.Get(orderLine.ProductId);
                    product.ItemsReserved -= orderLine.Quantity;
                    productRepos.Edit(product);
                }

                
            }
        }

        private void HandleOrderShipped(OrderStatusChangedMessage message)
        {
            using (var scope = provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var productRepos = services.GetService<IRepository<Product>>();

                foreach (var orderLine in message.OrderLines)
                {
                    var product = productRepos.Get(orderLine.ProductId);
                    product.ItemsInStock -= orderLine.Quantity;
                    product.ItemsReserved -= orderLine.Quantity;
                    productRepos.Edit(product);
                }
            }
        }
    }
}
