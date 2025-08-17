using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Net.Http.Headers;

namespace Common.Logging
{
    public static class LoggingSetup
    {
        private const string HeaderName = "X-Correlation-ID";

        public static void ConfigureLogging(this WebApplicationBuilder builder)
        {
            // Download config from NLog.config
            builder.Logging.ClearProviders();
            builder.Host.UseNLog(new NLogAspNetCoreOptions()
            {
                RemoveLoggerFactoryFilter = true
            });

            // MassTransit ConsumeObserver for correlationId
            builder.Services.AddSingleton<CorrelationConsumeObserver>();
        }

        // extension method для HTTP pipeline
        public static void UseCorrelationLogging(this WebApplication app)
        {
            // Middleware for CorrelationId
            app.UseMiddleware<CorrelationIdMiddleware>();


            // using this to add correlationId on "throw" errors
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                    var exception = exceptionHandlerPathFeature?.Error;

                    var correlationId = context.Items[HeaderName]?.ToString() ?? "no-correlation-id";
                    context.Response.Headers[HeaderName] = correlationId;

                    using (ScopeContext.PushProperty(HeaderName, correlationId)) { }

                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("Internal server error");
                });
            });
        }

    }
}