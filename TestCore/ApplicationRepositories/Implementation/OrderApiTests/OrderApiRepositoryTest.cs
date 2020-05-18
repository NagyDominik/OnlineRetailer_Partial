using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Moq;
using OrderApi.Data;
using OrderApi.Models;
using Xunit;

namespace TestCore.ApplicationRepositories.Implementation.OrderApiTests
{
    public class OrderApiRepositoryTest
    {
        #region MockData


        class OrderTestData : IEnumerable<Object[]>
        {
            private static Order o1 = new Order()
            {
                Id = 1,
                CustomerId = 2,
                Date = DateTime.Now.AddDays(-1),
                Status = Status.Completed,
                OrderLines = new List<OrderLine>()
            };


            private static Order o2 = new Order()
            {
                Id = 3,
                CustomerId = 3,
                Date = DateTime.Now.AddDays(-1),
                Status = Status.Shipped,
                OrderLines = new List<OrderLine>()
            };


            public IEnumerator<object[]> GetEnumerator()
            {
                o1.OrderLines.Add(new OrderLine() { 
                    Id = 1,
                    OrderId = 1,
                    ProductId = 1,
                    Quantity = 2,
                    Order = o1
                });
                o1.OrderLines.Add(new OrderLine()
                {
                    Id = 2,
                    OrderId = 2,
                    ProductId = 4,
                    Quantity = 15,
                    Order = o1
                });


                o2.OrderLines.Add(new OrderLine()
                {
                    Id = 1,
                    OrderId = 3,
                    ProductId = 5,
                    Quantity = 6,
                    Order = o2
                });

                o2.OrderLines.Add(new OrderLine()
                {
                    Id = 2,
                    OrderId = 5,
                    ProductId = 2,
                    Quantity = 15,
                    Order = o2
                });

                yield return new object[] {o1};
                yield return new object[] {o2};
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        #endregion

        #region GetAllCustomer

        [Fact]
        public void GetAllOrderTest()
        {
            //OrderTestData orderTestData = new OrderTestData();
            //var objects = orderTestData.ToList();

            //List<Customer> orders = new List<Customer>();

            //foreach (var item in objects)
            //{
            //    orders.Add((Order) item[0]);
            //}

            //Mock<DbSet<Order>> dbSetMock = new Mock<DbSet<Order>>();

            //dbSetMock.As<IQueryable<Order>>().Setup(x => x.Provider).Returns(orders.AsQueryable().Provider);
            //dbSetMock.As<IQueryable<Order>>().Setup(x => x.Expression).Returns(orders.AsQueryable().Expression);
            //dbSetMock.As<IQueryable<Order>>().Setup(x => x.ElementType).Returns(orders.AsQueryable().ElementType);
            //dbSetMock.As<IQueryable<Order>>().Setup(x => x.GetEnumerator())
            //    .Returns(orders.AsQueryable().GetEnumerator());

            //Mock<OrderApiContext> contextMock = new Mock<OrderApiContext>();

            //contextMock.Setup(x => x.Orders).Returns(dbSetMock.Object);

            //IRepository<Order> orderRepository = new OrderRepository(contextMock.Object);

            //List<Order> retrievedOrders = orderRepository.GetAll().ToList();

            //// Verify that the GetAll method is only called once
            //contextMock.Verify(x => x.Orders, Times.Once);

            //Assert.Equal(orders, retrievedOrders);
        }

        #endregion

        #region GetOrderByID

        [Fact]
        public void GetOrderByIdTest()
        {
            OrderTestData ordertestData = new OrderTestData();
            var objects = ordertestData.ToList();

            List<Order> orders = new List<Order>();

            foreach (var item in objects)
            {
                orders.Add((Order) item[0]);
            }

            Mock<DbSet<Order>> dbSetMock = new Mock<DbSet<Order>>();
            dbSetMock.As<IQueryable<Order>>().Setup(x => x.Provider).Returns(orders.AsQueryable().Provider);
            dbSetMock.As<IQueryable<Order>>().Setup(x => x.Expression).Returns(orders.AsQueryable().Expression);
            dbSetMock.As<IQueryable<Order>>().Setup(x => x.ElementType).Returns(orders.AsQueryable().ElementType);
            dbSetMock.As<IQueryable<Order>>().Setup(x => x.GetEnumerator()).Returns(orders.AsQueryable().GetEnumerator());

            //Get OrderLines to create a mock OrderLine dbSet
            List<OrderLine> orderLines = new List<OrderLine>();
            foreach (Order order in orders)
            {
                orderLines.AddRange(order.OrderLines);
            }

            Mock<DbSet<OrderLine>> orderLineMock = new Mock<DbSet<OrderLine>>();

            orderLineMock.As<IQueryable<OrderLine>>().Setup(x => x.Provider).Returns(orderLines.AsQueryable().Provider);
            orderLineMock.As<IQueryable<OrderLine>>().Setup(x => x.Expression).Returns(orderLines.AsQueryable().Expression);
            orderLineMock.As<IQueryable<OrderLine>>().Setup(x => x.ElementType).Returns(orderLines.AsQueryable().ElementType);
            orderLineMock.As<IQueryable<OrderLine>>().Setup(x => x.GetEnumerator()).Returns(orderLines.AsQueryable().GetEnumerator());

            Mock<OrderApiContext> contextMock = new Mock<OrderApiContext>();

            contextMock.Setup(x => x.Orders).Returns(dbSetMock.Object);
            contextMock.Setup(x => x.OrderLines).Returns(orderLineMock.Object);

            IRepository<Order> orderRepository = new OrderRepository(contextMock.Object);
            
            Order order1 = orderRepository.Get(1);

            Assert.Equal(1, order1.Id);
            contextMock.Verify(x => x.Orders, Times.Once);
        }

        #endregion
    }
}