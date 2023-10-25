using CircuitBreaker.Common;
using Microsoft.AspNetCore.Mvc;
using Silverback.Messaging.Broker;
using Silverback.Messaging.Publishing;

namespace CircuitBreaker.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ProducerController : ControllerBase
{
    [HttpGet]
    public async Task PublishAsync([FromServices] IPublisher publisher, [FromServices] IBroker broker)
    {
        if (!broker.IsConnected) return;

        await publisher.PublishAsync(
            new SampleMessage
            {
                Number = new Random(DateTime.Now.Millisecond).Next(1, 1000)
            });
    }
}