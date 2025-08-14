using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace ApiGateway.IntegrationTests
{
    public class JwksMockServer : IDisposable
    {
        private IHost _host;
        private readonly int _port;

        public string Url => $"http://localhost:{_port}";

        public JwksMockServer(int port = 12345)
        {
            _port = port;
        }

        public void Start()
        {
            _host = new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder
                        .UseKestrel()
                        .UseUrls(Url)
                        .ConfigureServices(services =>
                        {
                            services.AddRouting();
                        })
                        .Configure(app =>
                        {
                            app.UseRouting();
                            app.UseEndpoints(endpoints =>
                            {
                                endpoints.MapGet("/.well-known/openid-configuration/jwks", async context =>
                                {
                                    // this data is example of real AuthServer result, mainly to avoid error on program starting
                                    // because it need this server to get configs on start in AccountService
                                    var jwksJson = @"{
  ""keys"":[
    {
      ""kty"":""RSA"",
      ""use"":""sig"",
      ""kid"":""9525BAF42E2F2A70284A5D8AA0487B85"",
      ""e"":""AQAB"",
      ""n"":""o88t4Snb6NlYBdQK-hS0-so6D-vkuF7o30AqtNN-rhWVlTy3EMlOsfuJrmFFCoROcK96hA_BMw2jAivToChybLVZ1eiOeOnL5U5jmqszqF1Bdfjn9Rp-dsJgSmcDzlYfDPAc5gdloI9CVrrZdPfoTNRTlXYVc3v7EgfFEZs8z1oAT5_w8AP8AnXLrc88BZQ939u-YgckAB3VI5X6zP4PNyVCJxwTK7NhJuYTvLbmNvPCm6R5VJpAInl-qEB4e3vIjkmhuxCTBrOezuAItZsOqNPwS7jvZR3FwMEdZf-wvi4wPgEAlyyvMh0MFbcSZa_Z4aAAwd_Qsru_3jPQodbBDQ"",
      ""alg"":""RS256""
    },
    {
      ""kty"":""RSA"",
      ""use"":""sig"",
      ""kid"":""CFF92A36E9A153A93542A8BB3AC601B4"",
      ""e"":""AQAB"",
      ""n"":""19QyJ3N_jCxZx5zPu0ZqtVA5HRM3d44U7QvG0NnNOKJ-Di0QI8phEKmL1lPo-1au0LIQTDSxBuZqFRAeDFx_OfNLfBCOByz2rZEga0PTeQoOdZCjvcZoaFFaQDADCa56sln6PKw5ND6upCJGKmHh8nvA2oRRIdAWqorebPDqR9_rPXfEKLDq_ccNKwx70GL0XuUW_SFkW4ooI5JBZuVs1nBQo0UG4iEhOJBHteBcyO46eNyAe1AMTaJrOU-7uVQrY7_9cLprqu5RHEVOLXPHg7a-IOq5X51MrYfMVeQdBSGOd0_YfjRdjo1TS04suGOP8FRYvPz5fNNzTb8mM1OFpQ"",
      ""alg"":""RS256""
    }
  ]
}";
                                    context.Response.ContentType = "application/json";
                                    await context.Response.WriteAsync(jwksJson);
                                });
                            });
                        });
                })
                .Build();

            _host.Start();
        }

        public void Dispose()
        {
            _host?.StopAsync().GetAwaiter().GetResult();
            _host?.Dispose();
        }
    }
}
