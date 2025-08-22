using Dapper;
using RefactoringChallenge.Application.Interfaces.Data;
using RefactoringChallenge.Domain.Entities;
using System.Data;

namespace RefactoringChallenge.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly IDbConnection _connection;

        public ProductRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            const string sql = @"SELECT Id, Name, Category, Price
                                 FROM Products
                                 WHERE Id = @Id";

            return await _connection.QueryFirstOrDefaultAsync<Product>(sql, new { Id = id });
        }
    }
}
