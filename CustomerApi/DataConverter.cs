using DTOs;
using CustomerApi.Models;
using System;

namespace CustomerApi
{
    public class DataConverter
    {
        public Customer CustomerDTOToModel(CustomerDTO dto)
        {
            Customer ret = new Customer();
            ret.Id = dto.Id;
            ret.Name = dto.Name;
            ret.Email = dto.Email;
            ret.Email = dto.PhoneNumber;
            ret.BillingAddress = dto.BillingAddress;
            ret.ShippingAddress = dto.ShippingAddress;
            ret.CreditStanding = dto.CreditStanding;

            return ret;
        }

        public CustomerDTO ModelToCustomerDTO(Customer model)
        {
            CustomerDTO ret = new CustomerDTO();
            ret.Id = model.Id;
            ret.Name = model.Name;
            ret.Email = model.Email;
            ret.PhoneNumber = model.PhoneNumber;
            ret.BillingAddress = model.BillingAddress;
            ret.ShippingAddress = model.ShippingAddress;
            ret.CreditStanding = model.CreditStanding;

            return ret;
        }
    }
}
