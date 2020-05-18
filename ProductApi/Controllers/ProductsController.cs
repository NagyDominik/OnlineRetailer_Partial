using System.Collections.Generic;
using DTOs;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Data;
using ProductApi.Models;

namespace ProductApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IRepository<Product> repository;
        private readonly DataConverter converter;

        public ProductsController(IRepository<Product> repos)
        {
            repository = repos;
            converter = new DataConverter();
        }

        // GET products
        [HttpGet]
        public IEnumerable<ProductDTO> Get()
        {
            var models = repository.GetAll();
            List<ProductDTO> ret = new List<ProductDTO>();
            foreach (var model in models)
            {
                ret.Add(converter.ModelToProductDTO(model));
            }

            return ret;
        }

        // GET products/5
        [HttpGet("{id}", Name="GetProduct")]
        public IActionResult Get(int id)
        {
            var item = repository.Get(id);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(converter.ModelToProductDTO(item));
        }

        // POST products
        [HttpPost]
        public IActionResult Post([FromBody]ProductDTO product)
        {
            if (product == null)
            {
                return BadRequest();
            }

            var newProduct = repository.Add(converter.ProductDTOToModel(product));

            return CreatedAtRoute("GetProduct", new { id = newProduct.Id }, converter.ModelToProductDTO(newProduct));
        }

        // PUT products/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]ProductDTO product)
        {
            if (product == null || product.Id != id)
            {
                return BadRequest();
            }

            var modifiedProduct = repository.Get(id);

            if (modifiedProduct == null)
            {
                return NotFound();
            }

            modifiedProduct.Name = product.Name;
            modifiedProduct.Price = product.Price;
            modifiedProduct.ItemsInStock = product.ItemsInStock;
            modifiedProduct.ItemsReserved = product.ItemsReserved;

            repository.Edit(modifiedProduct);
            return new NoContentResult();
        }

        // DELETE products/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (repository.Get(id) == null)
            {
                return NotFound();
            }

            repository.Remove(id);
            return new NoContentResult();
        }
    }
}
