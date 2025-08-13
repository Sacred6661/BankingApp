using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.IntegrationTests
{
    [CollectionDefinition("Jwks server collection")]
    public class JwksServerCollection : ICollectionFixture<GlobalJwksServer> { }
}
