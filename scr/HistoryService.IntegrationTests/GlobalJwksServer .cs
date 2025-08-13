using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HistoryService.IntegrationTests
{
    public class GlobalJwksServer : IDisposable
    {
        public int Port { get; }
        public JwksMockServer Server { get; }

        public GlobalJwksServer()
        {
            Port = 12345;
            Server = new JwksMockServer(Port);

            Server.Start();
        }

        public void Dispose()
        {
            Server?.Dispose();
        }
    }

}
