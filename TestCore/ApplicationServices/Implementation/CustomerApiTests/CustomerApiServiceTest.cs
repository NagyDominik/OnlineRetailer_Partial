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
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
        public void GetAllUserTest()
        {
            CustomerTestData testData = new CustomerTestData();
            var objects = testData.ToList();

            List<Customer> customers = new List<Customer>();

            foreach (var item in objects)
            {
                customers.Add((Customer) item[0]);
            }

            Mock<DbSet<Customer>> dbSetMock = new Mock<DbSet<Customer>>();

            dbSetMock.As<IQueryable<Customer>>().Setup(x => x.Provider).Returns(customers.AsQueryable().Provider);
            dbSetMock.As<IQueryable<Customer>>().Setup(x => x.Expression).Returns(customers.AsQueryable().Expression);
            dbSetMock.As<IQueryable<Customer>>().Setup(x => x.ElementType).Returns(customers.AsQueryable().ElementType);
            dbSetMock.As<IQueryable<Customer>>().Setup(x => x.GetEnumerator())
                .Returns(customers.AsQueryable().GetEnumerator());

            Mock<CustomerApiContext> contextMock = new Mock<CustomerApiContext>();

            contextMock.Setup(x => x.Customers).Returns(dbSetMock.Object);

            IRepository<Customer> customerRepository = new CustomerRepository(contextMock.Object);

            List<Customer> retrievedCustomers = customerRepository.GetAll().ToList();

            // Verify that the GetAll method is only called once
            contextMock.Verify(x => x.Customers, Times.Once);

            Assert.Equal(customers, retrievedCustomers);
        }

        #endregion

        #region GetCustomerByID

        [Fact, Priority(0)]
        public void GetCustomerByIdTest()
        {
            CustomerTestData testData = new CustomerTestData();
            var objects = testData.ToList();

            List<Customer> customers = new List<Customer>();

            foreach (var item in objects)
            {
                customers.Add((Customer)item[0]);
            }

            Mock<DbSet<Customer>> dbSetMock = new Mock<DbSet<Customer>>();
            dbSetMock.As<IQueryable<Customer>>().Setup(x => x.Provider).Returns(customers.AsQueryable().Provider);
            dbSetMock.As<IQueryable<Customer>>().Setup(x => x.Expression).Returns(customers.AsQueryable().Expression);
            dbSetMock.As<IQueryable<Customer>>().Setup(x => x.ElementType).Returns(customers.AsQueryable().ElementType);
            dbSetMock.As<IQueryable<Customer>>().Setup(x => x.GetEnumerator())
                .Returns(customers.AsQueryable().GetEnumerator());


            Mock<CustomerApiContext> contextMock = new Mock<CustomerApiContext>();

            contextMock.Setup(x => x.Customers).Returns(dbSetMock.Object);

            IRepository<Customer> customerRepository = new CustomerRepository(contextMock.Object);

            Customer cust1 = customerRepository.Get(1);

            Assert.Equal(1, cust1.Id);
            contextMock.Verify(x=> x.Customers, Times.Once);

        }

        #endregion

        #region CreateCustomer

        [Fact, Priority(0)]
        public void AddCustomer()
        {
            CustomerTestData testData = new CustomerTestData();
            var objects = testData.ToList();

            List<Customer> customers = new List<Customer>();

            foreach (var item in objects)
            {
                customers.Add((Customer)item[0]);
            }

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

            Mock<DbSet<Customer>> dbSetMock = new Mock<DbSet<Customer>>();
            Mock<CustomerApiContext> contextMock = new Mock<CustomerApiContext>();

            contextMock.Setup(x => x.Customers).Returns(dbSetMock.Object);

            // Create a mock EntityEntry<Customer>
            Mock<IStateManager> iStateManager = new Mock<IStateManager>();
            Mock<Model> model = new Mock<Model>();

            Mock<EntityEntry<Customer>> custEntry = new Mock<EntityEntry<Customer>>(new InternalShadowEntityEntry(iStateManager.Object, new EntityType("Customer", model.Object, Microsoft.EntityFrameworkCore.Metadata.ConfigurationSource.Convention)));

            dbSetMock.Setup(x => x.Add(It.IsAny<Customer>())).Callback<Customer>(c => customers.Add(c)).Returns(custEntry.Object);

            IRepository<Customer> c = new CustomerRepository(contextMock.Object);
            
            Customer cust = c.Add(newCustomer);

            dbSetMock.Verify(x => x.Add(newCustomer), Times.Once);

        }

        #endregion

        #region DeleteCustomer

        [Fact, Priority(1)]
        public  void DeleteCustomer()
        {
            CustomerTestData testData = new CustomerTestData();
            var objects = testData.ToList();

            List<Customer> customers = new List<Customer>();

            foreach (var item in objects)
            {
                customers.Add((Customer)item[0]);
            }

            Mock<DbContext> context = new Mock<DbContext>();
            Mock<DbSet<Customer>> dbSetMock = new Mock<DbSet<Customer>>();
            context.Setup(x => x.Set<Customer>()).Returns(dbSetMock.Object);
            //dbSetMock.Setup(x => x.Remove(It.IsAny<Customer>())).Returns()

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
            Assert.Equal(customer.Email, customerUpdate.Email);
        }
    }
}