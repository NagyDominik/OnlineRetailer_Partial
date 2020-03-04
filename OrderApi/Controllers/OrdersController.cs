using System;
using System.Collections.Generic;
using System.Linq;
using DTOs;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Data;
using OrderApi.Infrastructure;
using OrderApi.Models;
using RestSharp;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IRepository<Order> repository;
        IServiceGateway<ProductDTO> productServiceGateway;
        IMessagePublisher messagePublisher;
        private readonly DataConverter converter;

        public OrdersController(IRepository<Order> repos, IServiceGateway<ProductDTO> gateway, IMessagePublisher publisher)
        {
            this.repository = repos;
            this.productServiceGateway = gateway;
            this.messagePublisher = publisher;
            this.converter = new DataConverter();
        }

        // GET: orders
        [HttpGet]
        public IEnumerable<OrderDTO> Get()
        {
            var models = repository.GetAll();
            List<OrderDTO> ret = new List<OrderDTO>();
            foreach (var model in models)
            {
                ret.Add(converter.ModelToOrderDTO(model));
            }

            return ret;
        }

        // GET orders/5
        [HttpGet("{id}", Name = "GetOrder")]
        public IActionResult Get(int id)
        {
            var item = repository.Get(id);
            if (item == null)
            {
                return NotFound();
            }

            return new ObjectResult(converter.ModelToOrderDTO(item));
        }

        // POST orders
        [HttpPost]
        public IActionResult Post([FromBody]OrderDTO order)
        {
            if (order == null)
            {
                return BadRequest();
            }

            if (ProductItemsAvailable(order))
            {
                try
                {
                    messagePublisher.PublishOrderStatusChangedMessage(order.CustomerId, order.OrderLines, "completed");
                    messagePublisher.PublishCustomerCreditStandingChangedMessage(order.CustomerId, false, "bad");

                    order.Status = DTOs.Status.Completed;
                    Order newOrder = repository.Add(converter.OrderDTOToModel(order));
                    return CreatedAtRoute("GetOrder", new { id = newOrder.Id }, newOrder);
                }
                catch
                {
                    return StatusCode(500, "An error happened. Try again.");
                }
            }
            else
            {
                return StatusCode(500, "Not enough products are available.");
            }
        }

        private bool ProductItemsAvailable(OrderDTO order)
        {
            foreach (OrderLineDTO orderLine in order.OrderLines)
            {
                ProductDTO orderedProduct = productServiceGateway.Get(orderLine.Id);
                if (orderLine.Quantity > orderedProduct.ItemsInStock - orderedProduct.ItemsReserved)
                {
                    return false;
                }
            }
            return true;
        }

        // PUT orders/5/cancel
        // This action method cancels an order and publishes an OrderStatusChangedMessage
        // with topic set to "cancelled".
        [HttpPut("{id}/cancel")]
        public IActionResult Cancel(int id)
        {
            throw new NotImplementedException();

            // Add code to implement this method.
        }

        // PUT orders/5/ship
        // This action method ships an order and publishes an OrderStatusChangedMessage.
        // with topic set to "shipped".
        [HttpPut("{id}/ship")]
        public IActionResult Ship(int id)
        {
            throw new NotImplementedException();

            // Add code to implement this method.
        }

        // PUT orders/5/pay
        // This action method marks an order as paid and publishes an OrderPaidMessage
        // (which have not yet been implemented). The OrderPaidMessage should specify the
        // Id of the customer who placed the order, and a number that indicates how many
        // unpaid orders the customer has (not counting cancelled orders). 
        [HttpPut("{id}/pay")]
        public IActionResult Pay(int id)
        {
            throw new NotImplementedException();

            // Add code to implement this method.
        }

    }
}
