using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using OrderApi.Models;
using System;

namespace OrderApi.Data
{
    public class OrderRepository : IRepository<Order>
    {
        private readonly OrderApiContext db;

        public OrderRepository(OrderApiContext context)
        {
            db = context;
        }

        Order IRepository<Order>.Add(Order entity)
        {
            if (entity.Date == null)
                entity.Date = DateTime.Now;
            
            var newOrder = db.Orders.Add(entity).Entity;
            db.SaveChanges();
            return newOrder;
        }

        void IRepository<Order>.Edit(Order entity)
        {
            db.Entry(entity).State = EntityState.Modified;
            db.SaveChanges();
        }

        Order IRepository<Order>.Get(int id)
        {
            Order order = db.Orders.FirstOrDefault(o => o.Id == id);

            List<OrderLine> orderLines = db.OrderLines.Where(o => o.OrderId == order.Id).ToList(); ;
            order.OrderLines = orderLines;

            return order;
        }

        IEnumerable<Order> IRepository<Order>.GetAll()
        {
            IEnumerable<Order> orders =  db.Orders.ToList();

            foreach (Order order in orders)
            {
                int orderId = order.Id;
                IEnumerable<OrderLine> orderLines = db.OrderLines.Where(l => l.OrderId == orderId);
                order.OrderLines = orderLines.ToList();
            }

            return orders;

        }

        void IRepository<Order>.Remove(int id)
        {
            var order = db.Orders.FirstOrDefault(p => p.Id == id);
            db.Orders.Remove(order);
            db.SaveChanges();
        }
    }
}
