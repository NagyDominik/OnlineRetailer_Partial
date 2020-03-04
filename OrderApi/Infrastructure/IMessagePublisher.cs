using DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderApi.Infrastructure
{
    public interface IMessagePublisher
    {
        void PublishOrderStatusChangedMessage(int? customerId, IList<OrderLineDTO> orderLines, string topic);

        void PublishOrderPaidMessage(int? custId, int unpaidOrders, string topic);
    }
}
