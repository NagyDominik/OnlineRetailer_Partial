using System;
using System.Threading;
using CustomerApi.Data;
using CustomerApi.Models;
using DTOs;
using EasyNetQ;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerApi.Infrastructure
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
                bus.Subscribe<CustomerCreditStandingChangedMessage>("customerApiCompleted", HandleOrderCompleted,
                    x => x = x.WithTopic("bad"));

                bus.Subscribe<CustomerCreditStandingChangedMessage>("customerApiCancelled", HandleOrderCancelled,
                    x => x = x.WithTopic("good"));

                bus.Subscribe<CustomerCreditStandingChangedMessage>("customerApiPaid", HandleOrderPaid,
                    x => x = x.WithTopic("good"));

                lock (this)
                {
                    Monitor.Wait(this);
                }
            }
        }

        //TODO: These methods can be merged into one which could handle all changes
        private void HandleOrderCompleted(CustomerCreditStandingChangedMessage message)
        {
            using (var scope = provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var customerRepos = services.GetService<IRepository<Customer>>();

                var customer = customerRepos.Get(message.CustomerId);
                customer.CreditStanding = message.CreditStanding;
                customerRepos.Edit(customer);
            }
        }

        private void HandleOrderCancelled(CustomerCreditStandingChangedMessage message)
        {
            using (var scope = provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var customerRepos = services.GetService<IRepository<Customer>>();

                var customer = customerRepos.Get(message.CustomerId);
                customer.CreditStanding = message.CreditStanding;
                customerRepos.Edit(customer);
            }
        }

        private void HandleOrderPaid(CustomerCreditStandingChangedMessage message)
        {
            using (var scope = provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var customerRepos = services.GetService<IRepository<Customer>>();

                var customer = customerRepos.Get(message.CustomerId);
                customer.CreditStanding = message.CreditStanding;
                customerRepos.Edit(customer);
            }
        }

    }
}
