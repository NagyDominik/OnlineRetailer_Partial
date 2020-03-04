using DTOs;
using EasyNetQ;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderApi.Infrastructure
{
    class MessagePublisher : IMessagePublisher, IDisposable
    {
        private readonly IBus bus;

        public MessagePublisher(string connString)
        {
            this.bus = RabbitHutch.CreateBus(connString);
        }

        public void Dispose()
        {
            if (bus != null)
            {
                bus.Dispose();
            }
        }

        public void PublishOrderPaidMessage(int? custId, int unpaidOrders, string topic)
        {
            OrderPaidMessage orderPaidMessage = new OrderPaidMessage
            {
                CustomerId = custId,
                UnpaidOrders = unpaidOrders
            };

            bus.Publish(orderPaidMessage, topic);
        }

        public void PublishOrderStatusChangedMessage(int? customerId, IList<OrderLineDTO> orderLines, string topic)
        {
            OrderStatusChangedMessage orderStatusChangedMessage = new OrderStatusChangedMessage
            {
                CustomerId = customerId,
                OrderLines = orderLines
            };

            bus.Publish(orderStatusChangedMessage, topic);
        }
    }
}
