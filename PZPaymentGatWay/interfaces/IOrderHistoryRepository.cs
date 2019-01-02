using PZPaymentGatWay.BObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PZPaymentGatWay.interfaces
{
    public interface IOrderHistoryRepository
    {
        Task<IEnumerable<OrderHistory>> GetAllOrderHistorys();
        Task<OrderHistory> GetOrderHistory(string id);

        // add new note document
        Task<object> AddOrderHistory(OrderHistory item);

        // remove a single document / note
        Task<bool> RemoveOrderHistory(string id);

        // update just a single document / note
        Task<bool> UpdateOrderHistory(string id, PosForm posForm);

        // demo interface - full document update
        Task<bool> UpdateOrderHistoryDocument(string id, PosForm posForm);

        // should be used with high cautious, only in relation with demo setup
        Task<bool> RemoveAllOrderHistorys();
    }
}
