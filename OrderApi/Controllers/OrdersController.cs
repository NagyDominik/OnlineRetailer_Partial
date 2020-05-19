using System;
using System.Collections.Generic;
using System.Linq;
using DTOs;
using Google.Api;
using Google.Api.Gax.ResourceNames;
using Google.Cloud.Monitoring.V3;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Data;
using OrderApi.Infrastructure;
using OrderApi.Models;
using Status = OrderApi.Models.Status;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IRepository<Order> repository;
        private readonly IServiceGateway<ProductDTO> productServiceGateway;
        private readonly IServiceGateway<CustomerDTO> customerServiceGateway;
        private readonly IMessagePublisher messagePublisher;
        private readonly DataConverter converter;

        public OrdersController(IRepository<Order> repos, IServiceGateway<ProductDTO> prudoctGateway, IServiceGateway<CustomerDTO> customerGateway, IMessagePublisher publisher)
        {
            repository = repos;
            productServiceGateway = prudoctGateway;
            customerServiceGateway = customerGateway;
            messagePublisher = publisher;
            converter = new DataConverter();
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

            CustomerStatus status = CheckCustomerStatus(order.CustomerId);

            switch (status)
            {
                case CustomerStatus.DoesNotExist:
                    return BadRequest("Customer does not exist!");
                case CustomerStatus.InBadCreditStanding:
                    return BadRequest("Customer is in a bad credit standing");
                default:
                    break;
            }

            if (ProductItemsAvailable(order))
            {
                try
                {
                    messagePublisher.PublishOrderStatusChangedMessage(order.CustomerId, order.OrderLines, "completed");

                    messagePublisher.PublishCustomerCreditStandingChangedMessage(order.CustomerId, false, "bad");

                    order.Status = DTOs.Status.Completed;

                    Order newOrder = converter.OrderDTOToModel(order);


                    newOrder = repository.Add(newOrder);
                    AddSalesDataPoint(newOrder);
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

        //Adds a data point using the Google Cloud monitoring API
        // Implementation for demonstration
        private void AddSalesDataPoint(Order newOrder)
        {
#if DEBUG
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", @"C:\Users\Peter\Downloads\Comp-Assignment-OnlineRetail-ab5561e2a955.json");
#endif

            string projectId = "resolute-land-274909";

            MetricServiceClient client = MetricServiceClient.Create();

            ProjectName name = new ProjectName(projectId);

            double sales = 0;


            foreach (var item in newOrder.OrderLines)
            {
                // This would be implemented by the product itself
                switch (item.ProductId)
                {
                    case 1:
                        sales += item.Quantity * 10;
                        break;
                    case 2:
                        sales += item.Quantity * 15;
                        break;
                    case 3:
                        sales += item.Quantity * 20;
                        break;
                    default:
                        sales = 10;
                        break;
                }
            }

            TypedValue salesTotal = new TypedValue
            {
                DoubleValue = sales
            };
            Point dataPoint = new Point
            {
                Value = salesTotal
            };

            Timestamp timestamp = new Timestamp
            {
                Seconds = (long)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds
            };

            TimeInterval interval = new TimeInterval
            {
                EndTime = timestamp
            };
            dataPoint.Interval = interval;

            Metric metric = new Metric
            {
                Type = "custom.googleapis.com/shops/daily_sales"
            };
            metric.Labels.Add("cust_id", newOrder.CustomerId.ToString());
            MonitoredResource resource = new MonitoredResource
            {
                Type = "global"
            };

            TimeSeries timeSeriesData = new TimeSeries
            {
                Metric = metric,
                Resource = resource
            };
            timeSeriesData.Points.Add(dataPoint);

            IEnumerable<TimeSeries> timeSeries = new List<TimeSeries> { timeSeriesData };

            client.CreateTimeSeries(name, timeSeries);
        }

        private bool ProductItemsAvailable(OrderDTO order)
        {
            foreach (OrderLineDTO orderLine in order.OrderLines)
            {
                ProductDTO orderedProduct = productServiceGateway.Get(orderLine.ProductId);
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
            try
            {
                Order order = repository.Get(id);

                order.Status = Models.Status.Canceled;
                repository.Edit(order);

                List<OrderLineDTO> orderLines = new List<OrderLineDTO>();

                foreach (OrderLine orderLine in order.OrderLines)
                {
                    orderLines.Add(converter.ModelToOrderLineDTO(orderLine));
                }

                messagePublisher.PublishOrderStatusChangedMessage(order.CustomerId, orderLines, "cancelled");

                messagePublisher.PublishCustomerCreditStandingChangedMessage(order.CustomerId, true, "good");
            }
            catch (Exception e)
            {
                return StatusCode(500, $"An error occured: {e.Message}");
            }

            return Ok();
        }

        // PUT orders/5/ship
        // This action method ships an order and publishes an OrderStatusChangedMessage.
        // with topic set to "shipped".
        [HttpPut("{id}/ship")]
        public IActionResult Ship(int id)
        {
            try
            {
                Order order = repository.Get(id);

                order.Status = Models.Status.Shipped;
                repository.Edit(order);

                List<OrderLineDTO> orderLines = new List<OrderLineDTO>();

                foreach (OrderLine orderLine in order.OrderLines)
                {
                    orderLines.Add(converter.ModelToOrderLineDTO(orderLine));
                }

                messagePublisher.PublishOrderStatusChangedMessage(order.CustomerId, orderLines, "shipped");

                SendEmailNotificationToCustomer(order.CustomerId);
            }
            catch (Exception e)
            {
                return StatusCode(500, $"An error occured: {e.Message}");
            }

            return Ok();
        }

        /// <summary>
        /// Mock method, represents an email sent to the customer upon shipping
        /// </summary>
        /// <param name="customerId">The id of the customer</param>
        private void SendEmailNotificationToCustomer(int customerId)
        {
            CustomerDTO customer = customerServiceGateway.Get(customerId);

            Console.WriteLine($"Sent confirmation email to {customer.Email}");
        }

        // PUT orders/5/pay
        // This action method marks an order as paid and publishes an OrderPaidMessage
        // (which have not yet been implemented). The OrderPaidMessage should specify the
        // Id of the customer who placed the order, and a number that indicates how many
        // unpaid orders the customer has (not counting cancelled orders). 
        [HttpPut("{id}/pay")]
        public IActionResult Pay(int id)
        {
            try
            {
                Order order = repository.Get(id);
                order.Status = Models.Status.Paid;

                repository.Edit(order);

                // This might have to be changed
                int unpaidOrders = GetUnpaidOrders(order.CustomerId);

                messagePublisher.PublishOrderPaidMessage(id, unpaidOrders, "paid"); // this message maybe not required to send

                messagePublisher.PublishCustomerCreditStandingChangedMessage(order.CustomerId, true, "good");

            }
            catch (Exception e)
            {
                return StatusCode(500, $"An error occured: {e.Message}");
            }

            return Ok();
        }

        private int GetUnpaidOrders(int customerId)
        {
            return repository.GetAll().Where(x => x.CustomerId == customerId).Count(x => !x.Status.Equals(Status.Paid) && !x.Status.Equals(Status.Canceled)); 
        }

        private CustomerStatus CheckCustomerStatus(int customerId)
        {
            CustomerDTO customerDTO = customerServiceGateway.Get(customerId);

            if (customerDTO == null)
            {
                return CustomerStatus.DoesNotExist;
            }

            if (!customerDTO.CreditStanding)
            {
                return CustomerStatus.InBadCreditStanding;
            }

            return CustomerStatus.Ok;
        }

        private enum CustomerStatus
        {
            DoesNotExist,
            InBadCreditStanding,
            Ok
        }
    }
}
