using CustomerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerApi.Data
{
    public class CustomerApiContext : DbContext
    {
        public CustomerApiContext(DbContextOptions<CustomerApiContext> options) : base(options) { }
        //empty constructor for test environment 
        protected CustomerApiContext() { }
        public virtual DbSet<Customer> Customers { get; set; }

        /**
         * It is supposed to set EntityState of an entity to Modified.
         */
        public void SetModified(Customer entity)
        {
            Entry(entity).State = EntityState.Modified;
        }
    }
}
