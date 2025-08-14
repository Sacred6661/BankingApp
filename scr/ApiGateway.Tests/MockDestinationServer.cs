using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace ApiGateway.IntegrationTests
{
    public class MockDestinationServer
    {
        public static IHost BuildMockDestinationHost(int port, string serviceName)
        {
            return Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls($"http://localhost:{port}");
                    webBuilder.Configure(app =>
                    {
                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapGet("/api/v1/" + serviceName + "/{*path}", async ctx =>
                            {
                                var path = ctx.Request.Path;
                                await ctx.Response.WriteAsync($"OK from {serviceName} - Path: {path}");
                            });
                        });
                    });
                })
                .Build();
        }
    }
}
