using DTOs;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderApi.Infrastructure
{
    class ProductServiceGateway : IServiceGateway<ProductDTO>
    {
        private Uri productUri;

        public ProductServiceGateway(Uri uri)
        {
            this.productUri = uri;
        }

        public ProductDTO Get(int id)
        {
            RestClient client = new RestClient(productUri);

            RestRequest request = new RestRequest(id.ToString(), Method.GET);
            IRestResponse<ProductDTO> response = client.Execute<ProductDTO>(request);

            return response.Data;
        }
    }
}
