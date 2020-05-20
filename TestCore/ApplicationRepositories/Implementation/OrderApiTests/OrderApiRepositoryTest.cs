using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Moq;
using OrderApi.Data;
using OrderApi.Models;
using Xunit;

namespace TestCore.ApplicationRepositories.Implementation.OrderApiTests
{
    public class OrderApiRepositoryTest
    {
        private readonly MockHelper helper = new MockHelper();

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

        class OrderLineTestData : IEnumerable<Object[]>
        {
            private static OrderLine ol1 = new OrderLine()
            {
                Id = 1,
                OrderId = 1,
                ProductId = 3,
                Quantity = 4
            };

            private static OrderLine ol2 = new OrderLine()
            {
                Id = 1,
                OrderId = 1,
                ProductId = 2,
                Quantity = 40,
            };

            private static OrderLine ol3 = new OrderLine()
            {
                Id = 1,
                OrderId = 3,
                ProductId = 5,
                Quantity = 6,
            };

            private static OrderLine ol4 = new OrderLine()
            {
                Id = 2,
                OrderId = 3,
                ProductId = 2,
                Quantity = 15,
            };

            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] {ol1};
                yield return new object[] {ol2};
                yield return new object[] {ol3};
                yield return new object[] {ol4};
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public List<Order> LoadOrders()
        {
            OrderTestData ordertestData = new OrderTestData();
            var objects = ordertestData.ToList();

            List<Order> orders = new List<Order>();

            foreach (var item in objects)
            {
                orders.Add((Order) item[0]);
            }

            return orders;
        }

        public List<OrderLine> LoadOrderLines()
        {
            OrderLineTestData orderLineTestData = new OrderLineTestData();
            var objects = orderLineTestData.ToList();

            List<OrderLine> orderLines = new List<OrderLine>();

            foreach (var item in objects)
            {
                orderLines.Add((OrderLine) item[0]);
            }

            return orderLines;
        }

        #endregion

        #region GetAllCustomer

        [Fact]
        public void GetAllOrderTest()
        {
            List<Order> orders = LoadOrders();
            List<OrderLine> orderLines = LoadOrderLines();

            foreach (var orderLine in orderLines)
            {
                orderLine.Order = orders.Find(order =>
                {
                    order.OrderLines.Add(orderLine);
                    return order.Id == orderLine.OrderId;
                });
            }

            Mock<DbSet<Order>> dbSetMockOrder = helper.GetQueryableMockDbSet(orders.ToArray());
            Mock<DbSet<OrderLine>> dbSetMockOrderLine = helper.GetQueryableMockDbSet(orderLines.ToArray());

            Mock<OrderApiContext> contextMock = new Mock<OrderApiContext>();

            contextMock.Setup(x => x.Orders).Returns(dbSetMockOrder.Object);
            contextMock.Setup(x => x.OrderLines).Returns(dbSetMockOrderLine.Object);
            OrderApi.Data.IRepository<Order> orderRepository = new OrderRepository(contextMock.Object);

            List<Order> retrievedOrders = orderRepository.GetAll().ToList();

            // Verify that the GetAll method is only called once
            contextMock.Verify(x => x.Orders, Times.Once);

            Assert.Equal(orders, retrievedOrders);
        }

        #endregion

        #region GetOrderByIDTest

        [Fact]
        public void GetOrderByIdTest()
        {
            List<Order> orders = LoadOrders();
            List<OrderLine> orderLines = LoadOrderLines();

            foreach (var orderLine in orderLines)
            {
                orderLine.Order = orders.Find(order =>
                {
                    order.OrderLines.Add(orderLine);
                    return order.Id == orderLine.OrderId;
                });
            }

            Mock<DbSet<Order>> dbSetMockOrder = helper.GetQueryableMockDbSet(orders.ToArray());
            Mock<DbSet<OrderLine>> dbSetMockOrderLine = helper.GetQueryableMockDbSet(orderLines.ToArray());

            Mock<OrderApiContext> contextMock = new Mock<OrderApiContext>();

            contextMock.Setup(x => x.Orders).Returns(dbSetMockOrder.Object);
            contextMock.Setup(x => x.OrderLines).Returns(dbSetMockOrderLine.Object);

            OrderApi.Data.IRepository<Order> orderRepository = new OrderRepository(contextMock.Object);

            Order order1 = orderRepository.Get(1);

            Assert.Equal(1, order1.Id);
            contextMock.Verify(x => x.Orders, Times.Once);
        }

        #endregion

        #region DeleteOrderTest

        [Fact]
        public void DeleteOrderTest()
        {
            List<Order> orders = LoadOrders();
            List<OrderLine> orderLines = LoadOrderLines();

            foreach (var orderLine in orderLines)
            {
                orderLine.Order = orders.Find(order =>
                {
                    order.OrderLines.Add(orderLine);
                    return order.Id == orderLine.OrderId;
                });
            }

            Mock<DbSet<Order>> dbSetMockOrder = helper.GetQueryableMockDbSet(orders.ToArray());
            Mock<DbSet<OrderLine>> dbSetMockOrderLine = helper.GetQueryableMockDbSet(orderLines.ToArray());

            Mock<OrderApiContext> contextMock = new Mock<OrderApiContext>();

            contextMock.Setup(x => x.Orders).Returns(dbSetMockOrder.Object);
            contextMock.Setup(x => x.OrderLines).Returns(dbSetMockOrderLine.Object);


            // Create a mock EntityEntry<Customer>
            Mock<IStateManager> iStateManager = new Mock<IStateManager>();
            Mock<Model> model = new Mock<Model>();

            Mock<EntityEntry<Order>> orderEntry = new Mock<EntityEntry<Order>>(
                new InternalShadowEntityEntry(iStateManager.Object,
                    new EntityType("Order", model.Object,
                        Microsoft.EntityFrameworkCore.Metadata.ConfigurationSource.Convention)));

            dbSetMockOrder.Setup(x => x.Remove(It.IsAny<Order>()))
                .Callback<Order>(c => orders.Remove(orders.Single(s => s.Id == c.Id)))
                .Returns(orderEntry.Object);

            OrderApi.Data.IRepository<Order> orderRepository = new OrderRepository(contextMock.Object);

            var c1 = orders[0];
            orderRepository.Remove(c1.Id);

            //contextMock.Verify(x => x.Customers, Times.Once);
            Assert.DoesNotContain(c1, orders);
            dbSetMockOrder.Verify(x => x.Remove(c1), Times.Once);
        }

        #endregion

        //[Fact]
        //public void AddOrder()
        //{
        //    List<Order> orders = LoadOrders();
        //    List<OrderLine> orderLines = LoadOrderLines();

        //    Order testOrder = new Order()
        //    {
        //        Id = 4,
        //        CustomerId = 2,
        //        Date = DateTime.Now.AddDays(-1),
        //        Status = Status.Completed,
        //        OrderLines = new List<OrderLine>()
        //    };

        //    //OrderLine testOrderLine = new OrderLine()
        //    //{
        //    //    Id = 3,
        //    //    OrderId = 4,
        //    //    ProductId = 17,
        //    //    Quantity = 89,
        //    //    Order = testOrder
        //    //};
        //    //testOrder.OrderLines.Add(testOrderLine);


        //    foreach (var orderLine in orderLines)
        //    {
        //        orderLine.Order = orders.Find(order =>
        //        {
        //            order.OrderLines.Add(orderLine);
        //            return order.Id == orderLine.OrderId;
        //        });
        //    }

        //    Mock<DbSet<Order>> dbSetMockOrder = helper.GetQueryableMockDbSet(orders.ToArray());
        //    Mock<DbSet<OrderLine>> dbSetMockOrderLine = helper.GetQueryableMockDbSet(orderLines.ToArray());

        //    Mock<OrderApiContext> contextMock = new Mock<OrderApiContext>();

        //    contextMock.Setup(x => x.Orders).Returns(dbSetMockOrder.Object);
        //    contextMock.Setup(x => x.OrderLines).Returns(dbSetMockOrderLine.Object);

        //    // Create a mock EntityEntry<Customer>
        //    Mock<IStateManager> iStateManager = new Mock<IStateManager>();
        //    Mock<Model> model = new Mock<Model>();

        //    Mock<EntityEntry<Order>> orderEntry = new Mock<EntityEntry<Order>>(
        //        new InternalShadowEntityEntry(iStateManager.Object,
        //            new EntityType("Order", model.Object,
        //                Microsoft.EntityFrameworkCore.Metadata.ConfigurationSource.Convention)));

        //    dbSetMockOrder.Setup(x => x.Add(It.IsAny<Order>())).Callback<Order>(o => orders.Add(o))
        //        .Returns(orderEntry.Object);


        //    IRepository<Order> orderRepository = new OrderRepository(contextMock.Object);

        //    Order order = orderRepository.Add(testOrder);

        //    Assert.Equal(testOrder, orders[3]);
        //    dbSetMockOrder.Verify(x => x.Add(testOrder), Times.Once);
        //}
    }
}