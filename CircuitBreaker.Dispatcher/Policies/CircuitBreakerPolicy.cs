using CircuitBreaker.Dispatcher.Extensions;
using Silverback.Diagnostics;
using Silverback.Messaging.Broker.Behaviors;
using Silverback.Messaging.Inbound.ErrorHandling;
using Silverback.Messaging.Messages;

namespace CircuitBreaker.Dispatcher.Policies;

public interface ICircuit : IDisposable
{
    string Id { get; }
    ECircuitState State { get; }
    bool IsOpen { get; }
    bool IsHalfOpen { get; }
    bool IsClosed { get; }

    int? FailedAttempts { get; }

    Task OpenAsync(CancellationToken cancellationToken = default);
    Task CloseAsync(CancellationToken cancellationToken = default);
    Task AttemptAsync(CancellationToken cancellationToken = default);
    Task RegisterFail(int failedAttempts);
}

public interface ICircuitRepository
{
    Task<ICircuit> GetOrAddByIdAsync(string id);
}

public enum ECircuitState
{
    Open,
    Closed,
    HalfOpen
}

public class CircuitBreakerPolicy : RetryableErrorPolicyBase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RetryErrorPolicy" /> class.
    /// </summary>
    /// <param name="initialDelay">
    ///     The optional delay to be applied to the first retry.
    /// </param>
    /// <param name="delayIncrement">
    ///     The optional increment to the delay to be applied at each retry.
    /// </param>
    public CircuitBreakerPolicy(TimeSpan? initialDelay = null, TimeSpan? delayIncrement = null)
    {
        InitialDelay = initialDelay ?? TimeSpan.Zero;
        DelayIncrement = delayIncrement ?? TimeSpan.Zero;

        if (InitialDelay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(initialDelay),
                initialDelay,
                "The specified initial delay must be greater than TimeSpan.Zero.");
        }

        if (DelayIncrement < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(delayIncrement),
                delayIncrement,
                "The specified delay increment must be greater than TimeSpan.Zero.");
        }
    }

    internal TimeSpan InitialDelay { get; }

    internal TimeSpan DelayIncrement { get; }

    /// <inheritdoc cref="ErrorPolicyBase.BuildCore" />
    protected override ErrorPolicyImplementation BuildCore(IServiceProvider serviceProvider) =>
        new CircuitBreakerPolicyImplementation(
            InitialDelay,
            DelayIncrement,
            MaxFailedAttemptsCount,
            ExcludedExceptions,
            IncludedExceptions,
            ApplyRule,
            MessageToPublishFactory,
            serviceProvider,
            serviceProvider.GetRequiredService<ICircuitRepository>(),
            serviceProvider.GetRequiredService<IInboundLogger<RetryErrorPolicy>>());

    private sealed class CircuitBreakerPolicyImplementation : ErrorPolicyImplementation
    {
        private readonly TimeSpan _initialDelay;

        private readonly TimeSpan _delayIncrement;
        private readonly ICircuitRepository _circuitRepository;

        private readonly IInboundLogger<RetryErrorPolicy> _logger;

        public CircuitBreakerPolicyImplementation(
            TimeSpan initialDelay,
            TimeSpan delayIncrement,
            int? maxFailedAttempts,
            ICollection<Type> excludedExceptions,
            ICollection<Type> includedExceptions,
            Func<IRawInboundEnvelope, Exception, bool>? applyRule,
            Func<IRawInboundEnvelope, Exception, object?>? messageToPublishFactory,
            IServiceProvider serviceProvider,
            ICircuitRepository circuitRepository,
            IInboundLogger<RetryErrorPolicy> logger)
            : base(
                maxFailedAttempts,
                excludedExceptions,
                includedExceptions,
                applyRule,
                messageToPublishFactory,
                serviceProvider,
                logger)
        {
            _initialDelay = initialDelay;
            _delayIncrement = delayIncrement;
            _circuitRepository = circuitRepository;
            _logger = logger;
        }

        protected override async Task<bool> ApplyPolicyAsync(
            ConsumerPipelineContext context,
            Exception exception)
        {
            //Check.NotNull(context, nameof(context));
            //Check.NotNull(exception, nameof(exception));

            if (!await TryRollbackAsync(context, exception).ConfigureAwait(false))
            {
                await context.Consumer.TriggerReconnectAsync().ConfigureAwait(false);
                return true;
            }

            var circuitId = context.GetCircuitBreakerId();
            if (!string.IsNullOrEmpty(circuitId))
            {
                var circuit = await _circuitRepository.GetOrAddByIdAsync(circuitId).ConfigureAwait(false);
                await circuit.RegisterFail(
                    Math.Max(context.Envelope.Headers.GetValueOrDefault<int>(DefaultMessageHeaders.FailedAttempts), 1)).ConfigureAwait(false);

                //await ApplyDelayAsync(context).ConfigureAwait(false);

                _logger.LogRetryProcessing(context.Envelope);
            }

            return true;
        }

        //[SuppressMessage("", "CA1031", Justification = Justifications.ExceptionLogged)]
        private async Task<bool> TryRollbackAsync(ConsumerPipelineContext context, Exception exception)
        {
            try
            {
                await context.TransactionManager.RollbackAsync(exception, stopConsuming: false)
                    .ConfigureAwait(false);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogRollbackToRetryFailed(context.Envelope, ex);
                return false;
            }
        }

        //private async Task ApplyDelayAsync(ConsumerPipelineContext context)
        //{
        //    var delay = (int)_initialDelay.TotalMilliseconds +
        //                (context.Envelope.Headers.GetValueOrDefault<int>(
        //                     DefaultMessageHeaders.FailedAttempts) *
        //                 (int)_delayIncrement.TotalMilliseconds);

        //    if (delay <= 0)
        //        return;

        //    _logger.LogInboundTrace(
        //        IntegrationLogEvents.RetryDelayed,
        //        context.Envelope,
        //        () => new object?[]
        //        {
        //            delay
        //        });

        //    await Task.Delay(delay).ConfigureAwait(false);
        //}
    }
}