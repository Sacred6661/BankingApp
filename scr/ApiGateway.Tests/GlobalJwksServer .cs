using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGateway.IntegrationTests
{
    public class GlobalJwksServer : IAsyncDisposable
    {
        public int Port { get; }
        public JwksMockServer Server { get; }

        private readonly List<IHost> _mocks;

        public GlobalJwksServer()
        {
            Port = 12345;
            Server = new JwksMockServer(Port);

            Server.Start();

            _mocks = new List<IHost>
            {
                MockDestinationServer.BuildMockDestinationHost(7127, "accounts"),
                MockDestinationServer.BuildMockDestinationHost(7212, "transactions"),
                MockDestinationServer.BuildMockDestinationHost(7213, "history"),
                MockDestinationServer.BuildMockDestinationHost(7118, "auth")
            };


            foreach (var mock in _mocks)
                mock.Start();
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var mock in _mocks)
                await mock.StopAsync();

            Server?.Dispose();
        }
    }

}
