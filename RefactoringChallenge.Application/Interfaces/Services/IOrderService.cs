using RefactoringChallenge.Domain.Entities;
namespace RefactoringChallenge.Application.Interfaces.Services;

/// <summary>
/// Service for working with orders
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Processes all pending orders for a given customer and saves the changes (applies discount, checks inventory, updates status, logs the process..).
    /// </summary>
    /// <param name="customer"></param>
    /// <returns></returns>
    Task<List<Order>> ProcessCustomerOrdersAsync(Customer customer);

    /// <summary>
    /// Gets customer's pending orders
    /// </summary>
    /// <param name="customerId">CustomerID</param>
    /// <param name="includeItemsWithProduct">Whether to include navigation products property</param>
    /// <returns>List of customer's pending orders</returns>
    Task<List<Order>> GetPendingOrdersByCustomerAsync(int customerId, bool includeItemsWithProduct);

    /// <summary>
    /// Updates orders in bulk
    /// </summary>
    /// <param name="orders">Order to update</param>
    /// <returns>Awaitable <see cref="Task"/></returns>
    Task UpdateOrdersBulkAsync(IEnumerable<Order> orders);
}
