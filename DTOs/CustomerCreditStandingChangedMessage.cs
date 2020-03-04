using System;
using System.Collections.Generic;
using System.Text;

namespace DTOs
{
    public class CustomerCreditStandingChangedMessage
    {
        public int CustomerId { get; set; }
        public bool CreditStanding { get; set; }
    }
}
