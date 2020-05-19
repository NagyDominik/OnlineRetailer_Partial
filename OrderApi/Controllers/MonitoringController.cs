using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Api;
using Google.Api.Gax;
using Google.Api.Gax.ResourceNames;
using Google.Cloud.Monitoring.V3;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OrderApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MonitoringController : ControllerBase
    {
        [HttpGet("/metrics")]
        public IActionResult Metrics()
        {
            MetricServiceClient client = MetricServiceClient.Create();
            ProjectName projectName = ProjectName.FromProject("resolute-land-274909");
            PagedEnumerable<ListMetricDescriptorsResponse, MetricDescriptor> metrics = client.ListMetricDescriptors(projectName);

            List<string> metricsList = new List<string>();

            foreach (MetricDescriptor descriptor in metrics.Take(10))
            {
                metricsList.Add($"{descriptor.Name}: {descriptor.DisplayName}");
            }

            return new ObjectResult(metricsList);

        }
    }
}