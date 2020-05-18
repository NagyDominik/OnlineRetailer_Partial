using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Moq;
using ProductApi.Data;
using ProductApi.Models;
using Xunit;

namespace TestCore.ApplicationRepositories.Implementation.ProductApiTests
{
    public class ProductApiRepositoryTest
    {
        readonly MockHelper helper = new MockHelper();
        private List<Product> products;

        #region MockData

        class ProductTestData : IEnumerable<Object[]>
        {
            private Product p1 = new Product()
                {Id = 1, Name = "Hammer", Price = 100, ItemsInStock = 10, ItemsReserved = 0};

            private Product p2 = new Product()
                {Id = 2, Name = "Screwdriver", Price = 70, ItemsInStock = 20, ItemsReserved = 0};

            private Product p3 = new Product()
                {Id = 3, Name = "Drill", Price = 500, ItemsInStock = 2, ItemsReserved = 0};

            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] {p1};
                yield return new object[] {p2};
                yield return new object[] {p3};
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        public List<Product> LoadCustomers()
        {
            ProductTestData testData = new ProductTestData();
            var objects = testData.ToList();

            List<Product> products = new List<Product>();

            foreach (var item in objects)
            {
                products.Add((Product)item[0]);
            }

            return products;
        }

        #endregion

        #region GetAllProductsTest

        [Fact]
        public void GetAllProductsTest()
        {
            List<Product> products = LoadCustomers();

            Mock<DbSet<Product>> dbSetMock = helper.GetQueryableMockDbSet(products.ToArray());

            Mock<ProductApiContext> contextMock = new Mock<ProductApiContext>();

            contextMock.Setup(x => x.Products).Returns(dbSetMock.Object);

            ProductApi.Data.IRepository<Product> productRepository = new ProductRepository(contextMock.Object);

            List<Product> retrievedProducts = productRepository.GetAll().ToList();

            // Verify that the GetAll method is only called once
            contextMock.Verify(x => x.Products, Times.Once);

            Assert.Equal(products, retrievedProducts);
        }

        #endregion

        #region GetProductByIdTest

        [Fact]
        public void GetProductByIdTest()
        {
            List<Product> products = LoadCustomers();

            Mock<DbSet<Product>> dbSetMock = helper.GetQueryableMockDbSet(products.ToArray());

            Mock<ProductApiContext> contextMock = new Mock<ProductApiContext>();

            contextMock.Setup(x => x.Products).Returns(dbSetMock.Object);

            ProductApi.Data.IRepository<Product> productRepository = new ProductRepository(contextMock.Object);

            Product prod1 = productRepository.Get(1);

            Assert.Equal(1, prod1.Id);
            contextMock.Verify(x => x.Products, Times.Once);
        }

        #endregion

        #region CreateProductTest

        [Fact]
        public void AddProduct()
        {
            List<Product> products = LoadCustomers();

            Product newProduct = new Product()
                {Id = 4, Name = "HammerTime", Price = 1200, ItemsInStock = 130, ItemsReserved = 10};

            Mock<DbSet<Product>> dbSetMock = new Mock<DbSet<Product>>();
            Mock<ProductApiContext> contextMock = new Mock<ProductApiContext>();

            contextMock.Setup(x => x.Products).Returns(dbSetMock.Object);

            // Create a mock EntityEntry<Customer>
            Mock<IStateManager> iStateManager = new Mock<IStateManager>();
            Mock<Model> model = new Mock<Model>();

            Mock<EntityEntry<Product>> custEntry = new Mock<EntityEntry<Product>>(
                new InternalShadowEntityEntry(iStateManager.Object,
                    new EntityType("Product", model.Object,
                        Microsoft.EntityFrameworkCore.Metadata.ConfigurationSource.Convention)));

            dbSetMock.Setup(x => x.Add(It.IsAny<Product>())).Callback<Product>(p => products.Add(p))
                .Returns(custEntry.Object);

            IRepository<Product> p = new ProductRepository(contextMock.Object);

            Product addedProduct = p.Add(newProduct);

            Assert.Equal(newProduct, products[3]);
            dbSetMock.Verify(x => x.Add(newProduct), Times.Once);
        }

        #endregion

        #region DeleteProductTest

        [Fact]
        public void DeleteProductTest()
        {
            List<Product> products = LoadCustomers();

            Mock<DbSet<Product>> dbSetMock = helper.GetQueryableMockDbSet(products.ToArray());

            Mock<ProductApiContext> contextMock = new Mock<ProductApiContext>();

            contextMock.Setup(x => x.Products).Returns(dbSetMock.Object);

            Mock<IStateManager> iStateManager = new Mock<IStateManager>();
            Mock<Model> model = new Mock<Model>();

            Mock<EntityEntry<Product>> custEntry = new Mock<EntityEntry<Product>>(
                new InternalShadowEntityEntry(iStateManager.Object,
                    new EntityType("Product", model.Object,
                        Microsoft.EntityFrameworkCore.Metadata.ConfigurationSource.Convention)));

            dbSetMock.Setup(x => x.Remove(It.IsAny<Product>()))
                .Callback<Product>(c => products.Remove(products.Single(s => s.Id == c.Id))).Returns(custEntry.Object);

            IRepository<Product> productRepository = new ProductRepository(contextMock.Object);

            var p1 = products[0];
            productRepository.Remove(p1.Id);

            //contextMock.Verify(x => x.Customers, Times.Once);
            Assert.DoesNotContain(p1, products);
            dbSetMock.Verify(x => x.Remove(p1), Times.Once);
        }

        #endregion
    }
}