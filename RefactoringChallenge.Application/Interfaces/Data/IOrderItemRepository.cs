using RefactoringChallenge.Domain.Entities;

namespace RefactoringChallenge.Application.Interfaces.Data;

public interface IOrderItemRepository
{
    Task<List<OrderItem>> GetByOrderIdAsync(int orderId);
}