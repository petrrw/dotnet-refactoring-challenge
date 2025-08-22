using Dapper;
using RefactoringChallenge.Application.Interfaces.Data;
using RefactoringChallenge.Domain.Entities;
using System.Data;
namespace RefactoringChallenge.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly IDbConnection _connection;

    public OrderRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        const string sql = @"SELECT Id, CustomerId, OrderDate, TotalAmount, Status,
                                        DiscountPercent, DiscountAmount
                                 FROM Orders 
                                 WHERE Id = @Id";

        return await _connection.QueryFirstOrDefaultAsync<Order>(sql, new { Id = id });
    }

    public async Task<List<Order>> GetPendingOrdersByCustomerAsync(int customerId)
    {
        const string sql = @"SELECT Id, CustomerId, OrderDate, TotalAmount, Status
                                 FROM Orders 
                                 WHERE CustomerId = @CustomerId AND Status = 'Pending'";

        return (List<Order>)await _connection.QueryAsync<Order>(sql, new { CustomerId = customerId });
    }

    public async Task UpdateOrderAsync(Order order)
    {
        // TODO: je potřeba setovat vše?
        const string sql = @"UPDATE Orders 
                                 SET TotalAmount = @TotalAmount, 
                                     DiscountPercent = @DiscountPercent, 
                                     DiscountAmount = @DiscountAmount, 
                                     Status = @Status 
                                 WHERE Id = @Id";

        await _connection.ExecuteAsync(sql, order);
    }
}
