using DTOs;
using OrderApi.Models;
using System;

namespace OrderApi
{
    public class DataConverter
    {
        public Order OrderDTOToModel(OrderDTO dto)
        {
            Order ret = new Order();
            ret.Id = dto.Id;
            ret.CustomerId = dto.CustomerId;
            ret.Date = dto.Date;
            Enum.TryParse(typeof(Models.Status), dto.Status.ToString(), out var convertedEnum);
            ret.Status = (Models.Status)convertedEnum;
            
            foreach (var orderline in dto.OrderLines)
            {
                OrderLine orderLineModel = OrderLineDTOToModel(dto.Id, orderline);
                orderLineModel.Order = ret;
                ret.OrderLines.Add(orderLineModel);
            }

            return ret;
        }

        public OrderLine OrderLineDTOToModel(int orderId, OrderLineDTO dto)
        {
            OrderLine ret = new OrderLine();
            ret.Id = dto.Id;
            ret.Quantity = dto.Quantity;
            ret.ProductId = dto.ProductId;
            ret.OrderId = orderId;

            return ret;
        }

        public OrderDTO ModelToOrderDTO(Order model)
        {
            OrderDTO ret = new OrderDTO();
            ret.Id = model.Id;
            ret.CustomerId = model.CustomerId;
            ret.Date = model.Date;
            Enum.TryParse(typeof(DTOs.Status), model.Status.ToString(), out var convertedEnum);
            ret.Status = (DTOs.Status)convertedEnum;

            foreach (var orderline in model.OrderLines)
            {
                ret.OrderLines.Add(ModelToOrderLineDTO(orderline));
            }

            return ret;
        }

        public OrderLineDTO ModelToOrderLineDTO(OrderLine model)
        {
            OrderLineDTO ret = new OrderLineDTO();
            ret.Id = model.Id;
            ret.Quantity = model.Quantity;
            ret.ProductId = model.ProductId;

            return ret;
        }
    }
}
