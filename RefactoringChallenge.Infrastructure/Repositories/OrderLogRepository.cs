using Dapper;
using RefactoringChallenge.Application.Interfaces.Data;
using RefactoringChallenge.Domain.Entities;
using System.Data;

namespace RefactoringChallenge.Infrastructure.Repositories;

public class OrderLogRepository : IOrderLogRepository
{
    private readonly IDbConnection _connection;

    public OrderLogRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task InsertAsync(OrderLog log)
    {
        const string sql = @"INSERT INTO OrderLogs (OrderId, LogDate, Message)
                                 VALUES (@OrderId, @LogDate, @Message)";

        await _connection.ExecuteAsync(sql, log);
    }
}
