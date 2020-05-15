using Microsoft.EntityFrameworkCore;
using ProductApi.Models;

namespace ProductApi.Data
{
    public class ProductApiContext : DbContext
    {
        public ProductApiContext(DbContextOptions<ProductApiContext> options) : base(options) { }
        protected ProductApiContext() { }

        public virtual DbSet<Product> Products { get; set; }
    }
}
