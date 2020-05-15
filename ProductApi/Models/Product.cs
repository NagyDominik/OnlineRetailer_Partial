namespace ProductApi.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int ItemsInStock { get; set; }
        public int ItemsReserved { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;

            Product other = (Product)obj;

            return this.Id == other.Id && string.Equals(this.Name, other.Name) && int.Equals(this.ItemsInStock, other.ItemsInStock) &&
                   int.Equals(this.ItemsReserved, other.ItemsReserved) && int.Equals(this.Price, other.Price);
        }

        public override int GetHashCode()
        {
            return new { Id, Name, ItemsReserved, Price, ItemsInStock }.GetHashCode();
        }
    }
}
