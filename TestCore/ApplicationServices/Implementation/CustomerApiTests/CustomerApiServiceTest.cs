using CustomerApi.Data;
using CustomerApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace TestCore.ApplicationServices.Implementation.CustomerApiTests
{
    public class CustomerApiServiceTest
    {
        #region MockData

        class CustomerTestData : IEnumerable<Object[]>
        {
            private Customer c1 = new Customer()
            {
                Id = 1,
                Name = "John Smith",
                Email = "john@mail.com",
                PhoneNumber = "+45525487",
                BillingAddress = "John Street 04",
                ShippingAddress = "John Street 04",
                CreditStanding = true
            };

            private Customer c2 = new Customer()
            {
                Id = 2,
                Name = "James Name",
                Email = "name@mail.com",
                PhoneNumber = "+45585466",
                BillingAddress = "Bondulance Street 04",
                ShippingAddress = "Stronk Street 04",
                CreditStanding = false
            };

            private Customer c3 = new Customer()
            {
                Id = 3,
                Name = "Bames Nond",
                Email = "nond@mail.com",
                PhoneNumber = "+45585487",
                BillingAddress = "Bondulance Street 33",
                ShippingAddress = "Stronk Street 45",
                CreditStanding = true
            };

            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] {c1};
                yield return new object[] {c2};
                yield return new object[] {c3};
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        /**
         * TestDb setup
         */
        private async Task<CustomerApiContext> GetDatabaseContext()
        {
            CustomerTestData testData = new CustomerTestData();
            var objects = testData.ToList();
            var options = new DbContextOptionsBuilder<CustomerApiContext>()
                .UseInMemoryDatabase(databaseName: "CustomersTestDb")
                .Options;
            var databaseContext = new CustomerApiContext(options);
            databaseContext.Database.EnsureCreated();
            if (await databaseContext.Customers.CountAsync() <= 0)
            {
                foreach (var item in objects)
                {
                    databaseContext.Customers.Add((Customer)item[0]);
                }
                await databaseContext.SaveChangesAsync();
            }
            return databaseContext;
        }

        #endregion

        #region GetCustomer

        [Fact]
        public async Task GetAllUserTest()
        {
            var dbContext = await GetDatabaseContext();
            IRepository<Customer> repo = new CustomerRepository(dbContext);


            CustomerTestData testData = new CustomerTestData();
            var objects = testData.ToList();

            List<Customer> customers = new List<Customer>();

            foreach (var item in objects)
            {
                customers.Add((Customer) item[0]);
            }
            
            List<Customer> retrievedCustomers = repo.GetAll().ToList();
            Assert.Equal(customers, retrievedCustomers);
        }

        
        #endregion
    }
}