using Microsoft.AspNetCore.Http;
using NLog;
using NLog.LayoutRenderers.Wrappers;

namespace Common.Logging
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string HeaderName = "X-Correlation-ID";

        public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            string correlationId = context.Request.Headers[HeaderName].FirstOrDefault()
                                   ?? Guid.NewGuid().ToString();

            context.Items[HeaderName] = correlationId;
            context.Response.Headers[HeaderName] = correlationId;

            using (ScopeContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }
    }
}
