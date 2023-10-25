using CircuitBreaker.Dispatcher.Interfaces;
using CircuitBreaker.Dispatcher.Policies;
using Confluent.Kafka;
using Silverback.Messaging.Configuration;

namespace CircuitBreaker.Dispatcher;

public class EndpointsConfigurator : IEndpointsConfigurator
{
    public void Configure(IEndpointsConfigurationBuilder builder)
    {
        var circuitBreakerPolicy = new CircuitBreakerPolicy(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));

        var circuitBreakerIdProvider = builder.ServiceProvider.GetRequiredService<ICircuitBreakerIdProvider>();
        circuitBreakerIdProvider
            .Register("samples-basic", "samples-basic-cb");

        builder
            .AddKafkaEndpoints(endpoints => endpoints
                // Configure the properties needed by all consumers/producers
                .Configure(config =>
                {
                    // The bootstrap server address is needed to connect
                    config.BootstrapServers = "PLAINTEXT://localhost:29092";
                })
                    
                // Consume the samples-basic topic
                .AddInbound(endpoint => endpoint
                    .ConsumeFrom("samples-basic")

                    .OnError(circuitBreakerPolicy)
                    
                    .Configure(config =>
                    {
                        // The consumer needs at least the bootstrap
                        // server address and a group id to be able
                        // to connect
                        config.GroupId = "sample-consumer";

                        // AutoOffsetReset.Earliest means that the
                        // consumer must start consuming from the
                        // beginning of the topic, if no offset was
                        // stored for this consumer group
                        config.AutoOffsetReset = AutoOffsetReset.Earliest;
                    })
                )
            );
    }
}
