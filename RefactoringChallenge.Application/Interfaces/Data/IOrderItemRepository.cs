using RefactoringChallenge.Domain.Entities;

namespace RefactoringChallenge.Application.Interfaces.Data;

/// <summary>
/// Repository for <see cref="OrderItem"/>
/// </summary>
public interface IOrderItemRepository
{
    /// <summary>
    /// Get OrderItems by order id
    /// </summary>
    /// <param name="orderId">OrderID</param>
    /// <returns>List of found <see cref="OrderItem"/></returns>
    Task<List<OrderItem>> GetByOrderIdAsync(int orderId);
}