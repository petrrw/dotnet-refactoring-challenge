using Dapper;
using RefactoringChallenge.Application.Interfaces.Data;
using RefactoringChallenge.Domain.Entities;
using System.Data;

namespace RefactoringChallenge.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly IDbConnection _connection;

    public CustomerRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        const string sql = @"SELECT Id, Name, Email, IsVip, RegistrationDate 
                                 FROM Customers 
                                 WHERE Id = @Id";

        return await _connection.QueryFirstOrDefaultAsync<Customer>(sql, new { Id = id });
    }
}
