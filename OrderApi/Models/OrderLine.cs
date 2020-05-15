using System.Text.Json.Serialization;

namespace OrderApi.Models
{
    public class OrderLine
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public int ProductId { get; set; }

        public int OrderId { get; set; }

        [JsonIgnore]
        public Order Order { get; set; }

    //    public override bool Equals(object obj)
    //    {
    //        if (ReferenceEquals(null, obj)) return false;
    //        if (ReferenceEquals(this, obj)) return true;
    //        if (obj.GetType() != this.GetType()) return false;

    //        Product other = (Product)obj;

    //        return this.Id == other.Id && string.Equals(this.Name, other.Name) && int.Equals(this.ItemsInStock, other.ItemsInStock) &&
    //               int.Equals(this.ItemsReserved, other.ItemsReserved) && int.Equals(this.Price, other.Price);
    //    }

    //    public override int GetHashCode()
    //    {
    //        return new { Id, Name, ItemsReserved, Price, ItemsInStock }.GetHashCode();
    //    }
    }
}
