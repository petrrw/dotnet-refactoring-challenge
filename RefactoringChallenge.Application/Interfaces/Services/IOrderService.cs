using RefactoringChallenge.Domain.Entities;
namespace RefactoringChallenge.Application.Interfaces.Services;

public interface IOrderService
{
    Task<List<Order>> ProcessCustomerOrdersAsync(Customer customer);
    Task<List<Order>> GetPendingOrdersByCustomerAsync(int customerId, bool includeItemsWithProduct);
    Task UpdateOrdersBulkAsync(IEnumerable<Order> orders);
}
