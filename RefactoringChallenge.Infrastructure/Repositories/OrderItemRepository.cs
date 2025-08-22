using Dapper;
using RefactoringChallenge.Application.Interfaces.Data;
using RefactoringChallenge.Domain.Entities;
using System.Data;

namespace RefactoringChallenge.Infrastructure.Repositories;

public class OrderItemRepository : IOrderItemRepository
{
    private readonly IDbConnection _connection;

    public OrderItemRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<List<OrderItem>> GetByOrderIdAsync(int orderId)
    {
        const string sql = @"SELECT Id, OrderId, ProductId, Quantity, UnitPrice 
                                 FROM OrderItems 
                                 WHERE OrderId = @OrderId";

        return (List<OrderItem>)await _connection.QueryAsync<OrderItem>(sql, new { OrderId = orderId });
    }
}
