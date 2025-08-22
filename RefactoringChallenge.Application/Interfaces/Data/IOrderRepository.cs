using RefactoringChallenge.Domain.Entities;

namespace RefactoringChallenge.Application.Interfaces.Data;

public interface IOrderRepository
{
    Task<List<Order>> GetPendingOrdersByCustomerAsync(int customerId);
    Task UpdateOrderAsync(Order order);
}
