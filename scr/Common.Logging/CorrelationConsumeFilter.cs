using MassTransit;
using Microsoft.Extensions.Logging;
using NLog;

namespace Common.Logging
{
    // if filter will be needed
    public class CorrelationConsumeFilter<T> : IFilter<ConsumeContext<T>> where T : class
    {
        public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
        {
            var correlationId = context.Headers.Get<string>("CorrelationId") ?? Guid.NewGuid().ToString();
            using (ScopeContext.PushProperty("CorrelationId", correlationId))
            {
                await next.Send(context);
            }
        }

        public void Probe(ProbeContext context) { }
    }
}
