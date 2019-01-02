using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using PZPaymentGatWay.BObjects;
using PZPaymentGatWay.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PZPaymentGatWay.Data
{

    public class OrderHistoryRepository : IOrderHistoryRepository
    {
        private readonly OrderHistoryContext _context = null;

        public OrderHistoryRepository(IOptions<Settings> settings)
        {
            _context = new OrderHistoryContext(settings);
        }

        public async Task<IEnumerable<OrderHistory>> GetAllOrderHistorys()
        {
            try
            {
                return await _context.OrderHistorys.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        public async Task<OrderHistory> GetOrderHistory(string id)
        {
            var filter = Builders<OrderHistory>.Filter.Eq("Id", id);

            try
            {
                return await _context.OrderHistorys
                                .Find(filter)
                                .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        public Task<object> AddOrderHistory(OrderHistory item)
        {
            try
            {
                return Task.FromResult(_context.OrderHistorys.InsertOneAsync(item).AsyncState);
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        public async Task<bool> RemoveOrderHistory(string id)
        {
            try
            {
                DeleteResult actionResult = await _context.OrderHistorys.DeleteOneAsync(
                     Builders<OrderHistory>.Filter.Eq("Id", id));

                return actionResult.IsAcknowledged
                    && actionResult.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        public async Task<bool> UpdateOrderHistory(string id, PosForm posForm)
        {
            var filter = Builders<OrderHistory>.Filter.Eq(s => s.Id, id);
            var update = Builders<OrderHistory>.Update
                            .Set(s => s.PosForm, posForm)
                            .CurrentDate(s => s.UpdatedOn);

            try
            {
                UpdateResult actionResult = await _context.OrderHistorys.UpdateOneAsync(filter, update);

                return actionResult.IsAcknowledged
                    && actionResult.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        public async Task<bool> UpdateOrderHistory(string id, OrderHistory item)
        {
            try
            {
                ReplaceOneResult actionResult = await _context.OrderHistorys
                                                .ReplaceOneAsync(n => n.Id.Equals(id)
                                                                , item
                                                                , new UpdateOptions { IsUpsert = true });
                return actionResult.IsAcknowledged
                    && actionResult.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        // Demo function - full document update
        public async Task<bool> UpdateOrderHistoryDocument(string id, PosForm posForm)
        {
            var item = await GetOrderHistory(id) ?? new OrderHistory();
            item.PosForm = posForm;
            item.UpdatedOn = DateTime.Now;

            return await UpdateOrderHistory(id, item);
        }

        public async Task<bool> RemoveAllOrderHistorys()
        {
            try
            {
                DeleteResult actionResult = await _context.OrderHistorys.DeleteManyAsync(new BsonDocument());

                return actionResult.IsAcknowledged
                    && actionResult.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }
    }
}
