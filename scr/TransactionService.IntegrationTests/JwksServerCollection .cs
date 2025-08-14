using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionService.IntegrationTests;

namespace AccountService.IntegrationTests
{
    [CollectionDefinition("Jwks server collection")]
    public class TransactionService : ICollectionFixture<GlobalJwksServer> { }
}
