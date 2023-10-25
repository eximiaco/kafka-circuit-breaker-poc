using CircuitBreaker.Common;

namespace CircuitBreaker.Dispatcher;

public class SampleMessageSubscriber
{
    private readonly ILogger<SampleMessageSubscriber> _logger;

    public SampleMessageSubscriber(ILogger<SampleMessageSubscriber> logger)
    {
        _logger = logger;
    }

    public void OnMessageReceived(SampleMessage message)
    {
        _logger.LogInformation("Received {MessageNumber}", message.Number);

        //throw new Exception("Request failed");

    }
}
