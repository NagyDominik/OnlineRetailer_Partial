using System.ComponentModel;

namespace CustomerApi.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string BillingAddress { get; set; }
        public string ShippingAddress { get; set; }
        [DefaultValue(true)]
        public bool CreditStanding { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;

            Customer other = (Customer)obj;

            return this.Id == other.Id && string.Equals(this.Name, other.Name) && string.Equals(this.Email, other.Email) &&
                    string.Equals(this.PhoneNumber, other.PhoneNumber) && string.Equals(this.BillingAddress, other.BillingAddress) &&
                    string.Equals(this.ShippingAddress, other.ShippingAddress) && this.CreditStanding == other.CreditStanding;
         }

        public override int GetHashCode()
        {
            return new { Id, Name, Email, PhoneNumber, BillingAddress, ShippingAddress, CreditStanding }.GetHashCode();
        }
    }
}
