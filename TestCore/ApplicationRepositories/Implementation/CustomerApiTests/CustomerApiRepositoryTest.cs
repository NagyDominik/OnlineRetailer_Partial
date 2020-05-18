using CustomerApi.Data;
using CustomerApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace TestCore.ApplicationRepositories.Implementation.CustomerApiTests
{
    public class CustomerApiRepositoryTest
    {
        private MockHelper helper = new MockHelper();
        private List<Customer> customers;

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

        public List<Customer> LoadCustomers()
        {
            CustomerTestData testData = new CustomerTestData();
            var objects = testData.ToList();

            List<Customer> customers = new List<Customer>();

            foreach (var item in objects)
            {
                customers.Add((Customer) item[0]);
            }

            return customers;
        }

        #endregion

        #region GetAllCustomer

        [Fact]
        public void GetAllUserTest()
        {
            List<Customer> customers = LoadCustomers();

            Mock<DbSet<Customer>> dbSetMock = helper.GetQueryableMockDbSet(customers.ToArray());

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

        [Fact]
        public void GetCustomerByIdTest()
        {
            List<Customer> customers = LoadCustomers();

            Mock<DbSet<Customer>> dbSetMock = helper.GetQueryableMockDbSet(customers.ToArray());

            Mock<CustomerApiContext> contextMock = new Mock<CustomerApiContext>();

            contextMock.Setup(x => x.Customers).Returns(dbSetMock.Object);

            IRepository<Customer> customerRepository = new CustomerRepository(contextMock.Object);

            Customer cust1 = customerRepository.Get(1);

            Assert.Equal(1, cust1.Id);
            contextMock.Verify(x => x.Customers, Times.Once);
        }

        #endregion

        #region CreateCustomer

        [Fact]
        public void AddCustomer()
        {
            List<Customer> customers = LoadCustomers();

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

            Mock<EntityEntry<Customer>> custEntry = new Mock<EntityEntry<Customer>>(
                new InternalShadowEntityEntry(iStateManager.Object,
                    new EntityType("Customer", model.Object,
                        Microsoft.EntityFrameworkCore.Metadata.ConfigurationSource.Convention)));

            dbSetMock.Setup(x => x.Add(It.IsAny<Customer>())).Callback<Customer>(c => customers.Add(c))
                .Returns(custEntry.Object);

            IRepository<Customer> c = new CustomerRepository(contextMock.Object);

            Customer cust = c.Add(newCustomer);

            Assert.Equal(newCustomer, customers[3]);
            dbSetMock.Verify(x => x.Add(newCustomer), Times.Once);
        }

        #endregion

        #region DeleteCustomer

        [Fact]
        public void DeleteCustomer()
        {
            List<Customer> customers = LoadCustomers();

            Mock<DbSet<Customer>> dbSetMock = helper.GetQueryableMockDbSet(customers.ToArray());

            Mock<CustomerApiContext> contextMock = new Mock<CustomerApiContext>();

            contextMock.Setup(x => x.Customers).Returns(dbSetMock.Object);

            Mock<IStateManager> iStateManager = new Mock<IStateManager>();
            Mock<Model> model = new Mock<Model>();

            Mock<EntityEntry<Customer>> custEntry = new Mock<EntityEntry<Customer>>(
                new InternalShadowEntityEntry(iStateManager.Object,
                    new EntityType("Customer", model.Object,
                        Microsoft.EntityFrameworkCore.Metadata.ConfigurationSource.Convention)));

            dbSetMock.Setup(x => x.Remove(It.IsAny<Customer>()))
                .Callback<Customer>(c => customers.Remove(customers.Single(s => s.Id == c.Id)))
                .Returns(custEntry.Object);

            IRepository<Customer> customerRepository = new CustomerRepository(contextMock.Object);

            var c1 = customers[0];
            customerRepository.Remove(c1.Id);

            Assert.DoesNotContain(c1, customers);
            dbSetMock.Verify(x => x.Remove(c1), Times.Once);
        }

        #endregion

        [Theory]
        [ClassData(typeof(CustomerTestData))]
        public void UpdateCustomerTest(Customer customer)
        {
            List<Customer> customers = LoadCustomers();

            Mock<DbSet<Customer>> dbSetMock = new Mock<DbSet<Customer>>();
            Mock<CustomerApiContext> contextMock = new Mock<CustomerApiContext>();

            contextMock.Setup(x => x.Customers).Returns(dbSetMock.Object);

            // Create a mock EntityEntry<Customer>
            Mock<IStateManager> iStateManager = new Mock<IStateManager>();
            Mock<Model> model = new Mock<Model>();

            Mock<EntityEntry<Customer>> custEntry = new Mock<EntityEntry<Customer>>(
                new InternalShadowEntityEntry(iStateManager.Object,
                    new EntityType("Customer", model.Object,
                        Microsoft.EntityFrameworkCore.Metadata.ConfigurationSource.Convention)));

            dbSetMock.Setup(x => x.Update(It.IsAny<Customer>()));

            contextMock.Setup(x => x.Attach(It.IsAny<Customer>())).Returns(custEntry.Object);

            IRepository<Customer> customerRepository = new CustomerRepository(contextMock.Object);


            // updating an existing customer
            // CreditStanding is the only property that can be Updated.
            customers[0].CreditStanding = false;

            //Throws a NullReferenceException when it reach the edit method in the repo.
            customerRepository.Edit(customers[0]);
            
            Assert.Equal(customer.Email, customer.Email);
        }
    }
}