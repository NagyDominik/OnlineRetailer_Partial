using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Moq;
using OrderApi.Data;
using OrderApi.Models;
using Xunit;

namespace TestCore.ApplicationServices.Implementation.OrderApiTests
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
                {
                    new OrderLine()
                    {
                        Id = 1,
                        OrderId = 1,
                        ProductId = 2,
                        Quantity = 4,
                        Order = new Order()
                        {
                            Id = o1.Id,
                            CustomerId = o1.CustomerId,
                            Date = o1.Date,
                            Status = o1.Status,
                            OrderLines = o1.OrderLines
                        }
                    },
                    new OrderLine()
                    {
                        Id = 1,
                        OrderId = 1,
                        ProductId = 2,
                        Quantity = 4,
                        Order = new Order()
                        {
                            Id = o1.Id,
                            CustomerId = o1.CustomerId,
                            Date = o1.Date,
                            Status = o1.Status,
                            OrderLines = o1.OrderLines
                        }
                    }
                },
            };

            private static Order o2 = new Order()
            {
                Id = 3,
                CustomerId = 3,
                Date = DateTime.Now.AddDays(-1),
                Status = Status.Shipped,
                OrderLines = new List<OrderLine>()
                {
                    new OrderLine()
                    {
                        Id = 1,
                        OrderId = 3,
                        ProductId = 5,
                        Quantity = 6,
                        Order = new Order()
                        {
                            Id = o2.Id,
                            CustomerId = o2.CustomerId,
                            Date = o2.Date,
                            Status = o2.Status,
                            OrderLines = o2.OrderLines
                        }
                    },
                    new OrderLine()
                    {
                        Id = 2,
                        OrderId = 5,
                        ProductId = 2,
                        Quantity = 15,
                        Order = new Order()
                        {
                            Id = o2.Id,
                            CustomerId = o2.CustomerId,
                            Date = o2.Date,
                            Status = o2.Status,
                            OrderLines = o2.OrderLines
                        }
                    }
                },
            };

            public IEnumerator<object[]> GetEnumerator()
            {
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
            dbSetMock.As<IQueryable<Order>>().Setup(x => x.GetEnumerator())
                .Returns(orders.AsQueryable().GetEnumerator());


            Mock<OrderApiContext> contextMock = new Mock<OrderApiContext>();

            contextMock.Setup(x => x.Orders).Returns(dbSetMock.Object);

            IRepository<Order> orderRepository = new OrderRepository(contextMock.Object);

            Order order1 = orderRepository.Get(1);

            Assert.Equal(1, order1.Id);
            contextMock.Verify(x => x.Orders, Times.Once);
        }

        #endregion
    }
}