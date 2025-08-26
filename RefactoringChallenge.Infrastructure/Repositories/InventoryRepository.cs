using Dapper;
using RefactoringChallenge.Application.Interfaces.Data;
using System.Data;
using System.Text;

namespace RefactoringChallenge.Infrastructure.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly IDbConnection _connection;

    public InventoryRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<Dictionary<int, int>> GetItemStockQuantitiesByIdsAsync(IEnumerable<int> ids)
    {
        const string sql = @"SELECT ProductId, StockQuantity
                         FROM Inventory
                         WHERE ProductId IN @ProductIds";

        var quantities = await _connection.QueryAsync<(int ProductId, int StockQuantity)>(
            sql, new { ProductIds = ids });

        return quantities.ToDictionary(x => x.ProductId, x => x.StockQuantity);
    }

    public async Task DecreaseStockBulkAsync(IEnumerable<(int ProductId, int Quantity)> items)
    {
        if (!items.Any()) return;

        var sql = new StringBuilder();
        sql.Append("UPDATE Inventory SET StockQuantity = CASE ProductId ");

        var parameters = new DynamicParameters();
        var ids = new List<int>();

        int i = 0;
        foreach (var item in items)
        {
            sql.Append($"WHEN @Id{i} THEN StockQuantity - @Qty{i} ");
            parameters.Add($"Id{i}", item.ProductId);
            parameters.Add($"Qty{i}", item.Quantity);
            ids.Add(item.ProductId);
            i++;
        }

        sql.Append("END WHERE ProductId IN @Ids");
        parameters.Add("Ids", ids);

        await _connection.ExecuteAsync(sql.ToString(), parameters);
    }
}
