using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using CustomerApi.Models;


namespace CustomerApi.Data
{
    public class CustomerRepository : IRepository<Customer>
    {
        private readonly CustomerApiContext db;

        public CustomerRepository(CustomerApiContext context)
        {
            db = context;
        }

        Customer IRepository<Customer>.Add(Customer entity)
        {
            var newCustomer = db.Customers.Add(entity).Entity;
            db.SaveChanges();
            return newCustomer;
        }

        void IRepository<Customer>.Edit(Customer entity)
        {
            db.Entry(entity).State = EntityState.Modified;
            db.SaveChanges();
        }

        Customer IRepository<Customer>.Get(int id)
        {
            return db.Customers.FirstOrDefault(c => c.Id == id);
        }

        IEnumerable<Customer> IRepository<Customer>.GetAll()
        {
            return db.Customers.ToList();
        }

        Customer IRepository<Customer>.Remove(int id)
        {
            var customer = db.Customers.Remove(new Customer(){Id = id}).Entity;
            db.SaveChanges();
            return customer;
        }
    }
}
