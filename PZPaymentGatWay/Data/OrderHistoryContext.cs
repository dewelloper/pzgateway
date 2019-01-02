using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PZPaymentGatWay.BObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PZPaymentGatWay.Data
{
    public class OrderHistoryContext
    {
        private readonly IMongoDatabase _database = null;

        public OrderHistoryContext(IOptions<Settings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            if (client != null)
                _database = client.GetDatabase(settings.Value.Database);
        }

        public IMongoCollection<OrderHistory> OrderHistorys
        {
            get
            {
                return _database.GetCollection<OrderHistory>("OrderHistory");
            }
        }
    }
}
