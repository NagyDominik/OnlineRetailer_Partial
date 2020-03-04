using System;
using System.Collections.Generic;
using System.Text;

namespace DTOs
{
    public class OrderStatusChangedMessage
    {
        public int? CustomerId { get; set; }
        public IList<OrderLineDTO> OrderLines { get; set; }
    }
}
