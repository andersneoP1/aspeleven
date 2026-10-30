using System.Threading.Tasks;
using DevShop.Common;
using DevShop.Models;

namespace DevShop.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<Order?> GetOrderWithDetailsAsync(int orderId);
        Task<PagedResult<Order>> GetOrdersWithDetailsPagedAsync(int page, int pageSize, int? userId = null);
    }
}
