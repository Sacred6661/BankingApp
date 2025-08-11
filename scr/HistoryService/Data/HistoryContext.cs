using HistoryService.Data.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;


namespace HistoryService.Data
{
    public class HistoryContext
    {
        private readonly IMongoClient _client;
        private readonly IMongoDatabase _database;

        public HistoryContext(IOptions<MongoDbSettings> mongoSettings)
        {
            _client = new MongoClient(mongoSettings.Value.ConnectionString);
            _database = _client.GetDatabase(mongoSettings.Value.DatabaseName);
        }

        public virtual IMongoCollection<HistoryEvent> HistoryEvents =>
            _database.GetCollection<HistoryEvent>("HistoryEvents");

        public async Task CreateCollectionIfNotExistsAsync(string collectionName)
        {
            var collections = await _database.ListCollectionNames().ToListAsync();
            if (!collections.Contains(collectionName))
            {
                await _database.CreateCollectionAsync(collectionName);
            }
        }

        public async Task DropCollectionAsync(string collectionName)
        {
            await _database.DropCollectionAsync(collectionName);
        }

        public async Task DropDatabaseAsync()
        {
            await _client.DropDatabaseAsync(_database.DatabaseNamespace.DatabaseName);
        }
    }

    public class MongoDbSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}
