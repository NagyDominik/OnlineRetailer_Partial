using System.Collections.Generic;
using System.Threading.Tasks;
using CustomerApi.Data;
using CustomerApi.Models;
using DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly IRepository<Customer> repository;
        private readonly DataConverter converter;

        public CustomersController(IRepository<Customer> repos)
        {
            repository = repos;
            converter = new DataConverter();
        }

        // GET: Customers
        [HttpGet]
        public IEnumerable<CustomerDTO> Get()
        {
            var models = repository.GetAll();
            List<CustomerDTO> ret = new List<CustomerDTO>();
            foreach (var model in models)
            {
                ret.Add(converter.ModelToCustomerDTO(model));
            }

            return ret;
        }

        // GET: Customers/5
        [HttpGet("{id}", Name = "Get")]
        public IActionResult Get(int id)
        {
            var customer = repository.Get(id);
            if (customer == null)
            {
                return NotFound();
            }
            return new ObjectResult(converter.ModelToCustomerDTO(customer));
        }

        // POST: Customers
        [HttpPost]
        public IActionResult Post([FromBody] CustomerDTO customer)     
        {
            if (customer == null)
            {
                return BadRequest("customer is null");
            }
            var newCustomer = repository.Add(converter.CustomerDTOToModel(customer));
            return new ObjectResult(newCustomer);
        }

        // PUT: Customers/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] CustomerDTO customer)
        {
            if (customer == null || customer.Id == id)
            {
                return BadRequest(" Customer is null or the ids are not identical");
            }

            var modifiedCustomer = repository.Get(id);
            if (modifiedCustomer == null)
            {
                return BadRequest(" The modified Customer is null");
            }
            modifiedCustomer.CreditStanding = customer.CreditStanding;
            
            repository.Edit(modifiedCustomer);
            return new NoContentResult();

        }

        // DELETE: Customers/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (repository.Get(id) == null)
            {
                return BadRequest("no customer with this id: " + id);
            }
            repository.Remove(id);
            return new NoContentResult();
        }
    }
}
