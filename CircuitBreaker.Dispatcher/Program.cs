using CircuitBreaker.Dispatcher;
using CircuitBreaker.Dispatcher.ConsumerBehaviors;
using CircuitBreaker.Dispatcher.Interfaces;
using CircuitBreaker.Dispatcher.Persistence.InMemory;
using CircuitBreaker.Dispatcher.Policies;
using CircuitBreaker.Dispatcher.Providers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ICircuitRepository, InMemoryCircuitRepository>();
builder.Services.AddSingleton<ICircuitBreakerIdProvider, CircuitBreakerIdProvider>();

builder.Services
    .AddSilverback()
    .WithConnectionToMessageBroker(opt => opt.AddKafka())
    .AddSingletonBrokerBehavior<AddCircuitBreakerHeaderConsumerBehavior>()
    .AddSingletonBrokerBehavior<CheckCircuitBreakerConsumerBehavior>()
    .AddSingletonBrokerBehavior<SuccessCircuitBreakerConsumerBehavior>()
    .AddEndpointsConfigurator<EndpointsConfigurator>()
    .AddSingletonSubscriber<SampleMessageSubscriber>();

var app = builder.Build();

app.Run();