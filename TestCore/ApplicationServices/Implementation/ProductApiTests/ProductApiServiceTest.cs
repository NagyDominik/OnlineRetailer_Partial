using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Moq;
using ProductApi.Data;
using ProductApi.Models;
using Xunit;

namespace TestCore.ApplicationServices.Implementation.ProductApiTests
{
    public class ProductApiServiceTest
    {
        #region MockData

        class ProductTestData : IEnumerable<Object[]>
        {
            private Product p1 = new Product() {Id = 1, Name = "Hammer", Price = 100, ItemsInStock = 10, ItemsReserved = 0};

            private Product p2 = new Product() {Id = 2, Name = "Screwdriver", Price = 70, ItemsInStock = 20, ItemsReserved = 0};

            private Product p3 = new Product() {Id = 3, Name = "Drill", Price = 500, ItemsInStock = 2, ItemsReserved = 0};

            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] {p1};
                yield return new object[] {p2};
                yield return new object[] {p3};
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        #endregion

        #region GetAllProducts
        [Fact]
        public void GetAllProducts()
        {
            ProductTestData testData = new ProductTestData();
            var objects = testData.ToList();

            List<Product> products = new List<Product>();

            foreach (var item in objects)
            {
                products.Add((Product) item[0]);
            }

            Mock<DbSet<Product>> dbSetMock = new Mock<DbSet<Product>>();

            dbSetMock.As<IQueryable<Product>>().Setup(x => x.Provider).Returns(products.AsQueryable().Provider);
            dbSetMock.As<IQueryable<Product>>().Setup(x => x.Expression).Returns(products.AsQueryable().Expression);
            dbSetMock.As<IQueryable<Product>>().Setup(x => x.ElementType).Returns(products.AsQueryable().ElementType);
            dbSetMock.As<IQueryable<Product>>().Setup(x => x.GetEnumerator())
                .Returns(products.AsQueryable().GetEnumerator());

            Mock<ProductApiContext> contextMock = new Mock<ProductApiContext>();

            contextMock.Setup(x => x.Products).Returns(dbSetMock.Object);

            IRepository<Product> productRepository = new ProductRepository(contextMock.Object);

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
            ProductTestData testData = new ProductTestData();
            var objects = testData.ToList();

            List<Product> products = new List<Product>();

            foreach (var item in objects)
            {
                products.Add((Product) item[0]);
            }

            Mock<DbSet<Product>> dbSetMock = new Mock<DbSet<Product>>();

            dbSetMock.As<IQueryable<Product>>().Setup(x => x.Provider).Returns(products.AsQueryable().Provider);
            dbSetMock.As<IQueryable<Product>>().Setup(x => x.Expression).Returns(products.AsQueryable().Expression);
            dbSetMock.As<IQueryable<Product>>().Setup(x => x.ElementType).Returns(products.AsQueryable().ElementType);
            dbSetMock.As<IQueryable<Product>>().Setup(x => x.GetEnumerator())
                .Returns(products.AsQueryable().GetEnumerator());

            Mock<ProductApiContext> contextMock = new Mock<ProductApiContext>();

            contextMock.Setup(x => x.Products).Returns(dbSetMock.Object);

            IRepository<Product> productRepository = new ProductRepository(contextMock.Object);

            Product prod1 = productRepository.Get(1);

            Assert.Equal(1, prod1.Id);
            contextMock.Verify(x => x.Products, Times.Once);
        }

        #endregion
    }
}