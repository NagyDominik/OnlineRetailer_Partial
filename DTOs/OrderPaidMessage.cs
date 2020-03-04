using System;
using System.Collections.Generic;
using System.Text;

namespace DTOs
{
    public class OrderPaidMessage
    {
        public int? CustomerId { get; set; }
        public int UnpaidOrders { get; set; }
    }
}
