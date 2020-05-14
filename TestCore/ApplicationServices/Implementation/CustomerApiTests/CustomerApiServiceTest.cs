using CustomerApi.Data;
using CustomerApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;
using Moq;
using NSubstitute;
using Xunit;
using Xunit.Priority;

namespace TestCore.ApplicationServices.Implementation.CustomerApiTests
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
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

        class CustomerUpdateData : IEnumerable<Object[]>
        {
            private Customer c1 = new Customer()
            {
                Id = 1,
                Name = "Johnny SmithUpdate",
                Email = "johny@hotmail.comUpdate",
                PhoneNumber = "+45525487",
                BillingAddress = "John Street 05Update",
                ShippingAddress = "John Street 09Update",
                CreditStanding = false
            };

            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] {c1};
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
                    databaseContext.Customers.Add((Customer) item[0]);
                }
                await databaseContext.SaveChangesAsync();
            }
            return databaseContext;
        }

        #endregion

        #region GetAllCustomer

        [Fact, Priority(-10)]
        public async Task GetAllUserTest()
        {
            CustomerTestData testData = new CustomerTestData();
            var objects = testData.ToList();

            List<Customer> customers = new List<Customer>();

            foreach (var item in objects)
            {
                customers.Add((Customer)item[0]);
            }

            var dbContext = await GetDatabaseContext();
            IRepository<Customer> repo = new CustomerRepository(dbContext);

            List<Customer> retrievedCustomers = repo.GetAll().ToList();
            Assert.Equal(customers, retrievedCustomers);
        }

        #endregion

        #region GetCustomerByID

        [Fact, Priority(0)]
        public async void GetUserByIdTest()
        {
            CustomerTestData testData = new CustomerTestData();
            var objects = testData.ToList();

            List<Customer> customers = new List<Customer>();

            foreach (var item in objects)
            {
                customers.Add((Customer) item[0]);
            }

            var dbContext = await GetDatabaseContext();

            IRepository<Customer> repo = new CustomerRepository(dbContext);

            for (int i = 0; i < customers.Count; i++)
            {
                int id = i + 1;
                Customer retrievedUser = repo.Get(id);
                Assert.Equal(id, retrievedUser.Id);
            }
        }

        #endregion

        #region CreateCustomer

        [Fact, Priority(0)]
        public async void AddUser()
        {
            Customer newCustomer = new Customer()
            {
                Id = 4,
                Name = "Hakuna Matata",
                Email = "hakuna@mail.com",
                PhoneNumber = "+455853287",
                BillingAddress = "Hakunamatata Street 33",
                ShippingAddress = "Stronk Street 48",
                CreditStanding = true
            };

            var dbContext = await GetDatabaseContext();
            IRepository<Customer> repo = new CustomerRepository(dbContext);

            repo.Add(newCustomer);
            Assert.Equal(newCustomer, repo.Get(newCustomer.Id));

            //removing the customer 
            repo.Remove(newCustomer.Id);
            List<Customer> retrievedCustomers = repo.GetAll().ToList();
            Assert.DoesNotContain(newCustomer, retrievedCustomers);
        }
        #endregion

        #region DeleteCustomer

        [Fact, Priority(1)]
        public async void DeleteCustomer()
        {
            

            var dbContext = await GetDatabaseContext();
            IRepository<Customer> repo = new CustomerRepository(dbContext);
            Customer cust = repo.Get(2);

            //removing the customer 
            repo.Remove(cust.Id);
            List<Customer> retrievedCustomers = repo.GetAll().ToList();
            Assert.DoesNotContain(cust, retrievedCustomers);
        }

        #endregion

        [Theory, Priority(0)]
        [ClassData(typeof(CustomerUpdateData))]
        public async void UpdateUserTest(Customer customer)
        {
            var dbContext = await GetDatabaseContext();
            IRepository<Customer> repo = new CustomerRepository(dbContext);

            var customerUpdate = repo.Get(customer.Id);

            // updating an existing customer
            customerUpdate.CreditStanding = customer.CreditStanding;
            customerUpdate.Email = customer.Email;
            customerUpdate.BillingAddress = customer.BillingAddress;
            customerUpdate.Name = customer.Name;
            
            repo.Edit(customerUpdate);
            Assert.Equal(customer.Email ,customerUpdate.Email);
        }
    }
}