using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Api;
using Google.Api.Gax;
using Google.Api.Gax.ResourceNames;
using Google.Cloud.Monitoring.V3;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace OrderApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MonitoringController : ControllerBase
    {

        private const string PROJECT_ID = "resolute-land-274909";

        private readonly string[] metrics = new string[]
        {
            "compute.googleapis.com/instance/cpu/utilization",
            "custom.googleapis.com/shops/daily_sales",
        };

        private static MetricServiceClient client;

        public MonitoringController()
        {
#if DEBUG
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", @"C:\Users\Peter\Downloads\Comp-Assignment-OnlineRetail-ab5561e2a955.json");
#endif

            client = MetricServiceClient.Create();
        }

        [HttpGet("{id}")]
        public IActionResult Metrics(int id)
        {
            string data = GetMetrics(id);

            return new ObjectResult(data);
        }

        private string GetMetrics(int id)
        {
            string metricType = metrics[id - 1];
            string filter = $"metric.type=\"{metricType}\"";

            ListTimeSeriesRequest request = new ListTimeSeriesRequest()
            {
                ProjectName = new ProjectName(PROJECT_ID),
                Filter = filter,
                Interval = CreateInterval(180),
                View = ListTimeSeriesRequest.Types.TimeSeriesView.Full
            };

            PagedEnumerable<ListTimeSeriesResponse, TimeSeries> response = client.ListTimeSeries(request);

            StringBuilder sb = new StringBuilder();

            foreach (TimeSeries series in response)
            {
                sb.Append(JObject.Parse(series.ToString()));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Create an interval. The start of the interval will be: currentTime - minutes, the end of the interval will be: currentTime.
        /// </summary>
        /// <param name="minutes">The lenght of the interval in minutes.</param>
        private TimeInterval CreateInterval(int minutes)
        {
            long timestamp = (long)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds;

            Timestamp startTimeStamp = new Timestamp
            {
                Seconds = timestamp - (60 * minutes)
            };

            Timestamp endTimeStamp = new Timestamp
            {
                Seconds = timestamp
            };

            TimeInterval interval = new TimeInterval
            {
                StartTime = startTimeStamp,
                EndTime = endTimeStamp
            };

            return interval;
        }
    }
}