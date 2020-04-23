using DTOs;
using ProductApi.Models;

namespace ProductApi
{
    public class DataConverter
    {
        public Product ProductDTOToModel(ProductDTO dto)
        {
            Product ret = new Product();
            ret.Id = dto.Id;
            ret.Name = dto.Name;
            ret.Price = dto.Price;
            ret.ItemsInStock = dto.ItemsInStock;
            ret.ItemsReserved = dto.ItemsReserved;

            return ret;
        }

        public ProductDTO ModelToProductDTO(Product model)
        {
            ProductDTO ret = new ProductDTO();
            ret.Id = model.Id;
            ret.Name = model.Name;
            ret.Price = model.Price;
            ret.ItemsInStock = model.ItemsInStock;
            ret.ItemsReserved = model.ItemsReserved;

            return ret;
        }
    }
}
