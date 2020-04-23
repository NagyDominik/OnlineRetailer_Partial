using System.Collections.Generic;

namespace DTOs
{
    public class OrderStatusChangedMessage
    {
        public int? CustomerId { get; set; }
        public IList<OrderLineDTO> OrderLines { get; set; }
    }
}
