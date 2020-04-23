using OrderApi.Infrastructure;
using RestSharp;
using System;

namespace DTOs
{
    class CustomerServiceGateway : IServiceGateway<CustomerDTO>
    {
        private readonly Uri customerUri;

        public CustomerServiceGateway(Uri uri)
        {
            this.customerUri = uri;
        }

        public CustomerDTO Get(int id)
        {
            RestClient client = new RestClient(customerUri);

            RestRequest request = new RestRequest(id.ToString(), Method.GET);
            IRestResponse<CustomerDTO> response = client.Execute<CustomerDTO>(request);

            return response.Data;
        }
    }
}
