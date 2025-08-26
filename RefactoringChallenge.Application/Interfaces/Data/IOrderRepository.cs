using RefactoringChallenge.Domain.Entities;

namespace RefactoringChallenge.Application.Interfaces.Data;

/// <summary>
/// Repository for orders
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Gets cusomer's pending orders
    /// </summary>
    /// <param name="customerId">CustomerID</param>
    /// <param name="includeItemsWithProduct">Whether to include navigation product property</param>
    /// <returns>Found customer's orders</returns>
    Task<List<Order>> GetPendingOrdersByCustomerAsync(int customerId, bool includeItemsWithProduct);

    /// <summary>
    /// Updates orders in bulk
    /// </summary>
    /// <param name="orders">Orders to update</param>
    /// <returns>Awaitable <see cref="Task"/></returns>
    Task UpdateOrdersBulkAsync(IEnumerable<Order> orders);
}
