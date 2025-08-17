    using MassTransit;
    using NLog;

    namespace Common.Logging
    {
        // To use for consumers to add correlation info
        public class CorrelationConsumeObserver : IConsumeObserver
        {
            public Task PreConsume<T>(ConsumeContext<T> context) where T : class
            {
                // Витягуємо CorrelationId з заголовків або генеруємо новий
                var correlationId = context.Headers.Get<string>("CorrelationId") ?? Guid.NewGuid().ToString();

                // Додаємо у ScopeContext для NLog
                ScopeContext.PushProperty("CorrelationId", correlationId);

                return Task.CompletedTask;
            }

            public Task PostConsume<T>(ConsumeContext<T> context) where T : class
            {
                return Task.CompletedTask;
            }

            public Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception) where T : class
            {
                return Task.CompletedTask;
            }
        }
    }
