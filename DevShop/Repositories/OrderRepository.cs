using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DevShop.Common;
using DevShop.Models;

namespace DevShop.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(ShopDbContext context) : base(context)
        {
        }

        public async Task<Order?> GetOrderWithDetailsAsync(int orderId)
        {
            return await _dbSet
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.IdOrders == orderId);
        }

        public async Task<PagedResult<Order>> GetOrdersWithDetailsPagedAsync(int page, int pageSize, int? userId = null)
        {
            IQueryable<Order> query = _dbSet
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.User);

            if (userId.HasValue)
            {
                query = query.Where(o => o.UserId == userId.Value);
            }

            int totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Order>(items, totalCount, page, pageSize);
        }
    }
}
