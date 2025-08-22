using Dapper;
using RefactoringChallenge.Application.Interfaces.Data;
using System.Data;

namespace RefactoringChallenge.Infrastructure.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly IDbConnection _connection;

    public InventoryRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<int?> GetStockQuantityAsync(int productId)
    { // TODO: přejmenovat, zjistuje to jen jestli all products available
        const string sql = @"SELECT StockQuantity 
                                 FROM Inventory 
                                 WHERE ProductId = @ProductId";

        return await _connection.QueryFirstOrDefaultAsync<int?>(sql, new { ProductId = productId });
    }

    public async Task DecreaseStockAsync(int productId, int quantity)
    {
        const string sql = @"UPDATE Inventory 
                                 SET StockQuantity = StockQuantity - @Quantity 
                                 WHERE ProductId = @ProductId";

        await _connection.ExecuteAsync(sql, new { ProductId = productId, Quantity = quantity });
    }
}
