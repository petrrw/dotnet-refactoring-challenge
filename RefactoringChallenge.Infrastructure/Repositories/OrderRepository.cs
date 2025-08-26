using Dapper;
using RefactoringChallenge.Application.Interfaces.Data;
using RefactoringChallenge.Domain.Entities;
using System.Data;
using System.Text;
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

    public async Task<List<Order>> GetPendingOrdersByCustomerAsync(int customerId, bool includeItemsWithProduct)
    {
        if (!includeItemsWithProduct)
            throw new NotImplementedException();

        const string sql = @"
        SELECT 
            o.Id, o.CustomerId, o.OrderDate, o.TotalAmount, o.Status,
            oi.Id, oi.OrderId, oi.Quantity, oi.UnitPrice, oi.ProductId,
            p.Id, p.Name, p.Price, p.Category
        FROM Orders o
        LEFT JOIN OrderItems oi ON o.Id = oi.OrderId
        LEFT JOIN Products p ON oi.ProductId = p.Id
        WHERE o.CustomerId = @CustomerId
          AND o.Status = 'Pending';
    ";

        var orderDictionary = new Dictionary<int, Order>();

        var orders = await _connection.QueryAsync<Order, OrderItem, Product, Order>(
            sql,
            (order, orderItem, product) =>
            {
                if (!orderDictionary.TryGetValue(order.Id, out var orderEntry))
                {
                    orderEntry = order;
                    orderEntry.Items = new List<OrderItem>();
                    orderDictionary.Add(orderEntry.Id, orderEntry);
                }

                if (orderItem != null)
                {
                    orderItem.Product = product;
                    orderEntry.Items.Add(orderItem);
                }

                return orderEntry;
            },
            new { CustomerId = customerId },
            splitOn: "Id,Id"
        );

        return orderDictionary.Values.ToList();
    }

    public async Task UpdateOrdersBulkAsync(IEnumerable<Order> orders)
    {
        if (!orders.Any()) return;

        var sql = new StringBuilder("UPDATE Orders SET ");
        var parameters = new DynamicParameters();
        var ids = new List<int>();

        // Creates CASE statements for columns
        sql.Append("TotalAmount = CASE Id ");
        int i = 0;
        foreach (var order in orders)
        {
            sql.Append($"WHEN @Id{i} THEN @TotalAmount{i} ");
            parameters.Add($"Id{i}", order.Id);
            parameters.Add($"TotalAmount{i}", order.TotalAmount);
            ids.Add(order.Id);
            i++;
        }
        sql.Append("END, ");

        // DiscountPercent
        sql.Append("DiscountPercent = CASE Id ");
        i = 0;
        foreach (var order in orders)
        {
            sql.Append($"WHEN @Id{i} THEN @DiscountPercent{i} ");
            parameters.Add($"DiscountPercent{i}", order.DiscountPercent);
            i++;
        }
        sql.Append("END, ");

        // DiscountAmount
        sql.Append("DiscountAmount = CASE Id ");
        i = 0;
        foreach (var order in orders)
        {
            sql.Append($"WHEN @Id{i} THEN @DiscountAmount{i} ");
            parameters.Add($"DiscountAmount{i}", order.DiscountAmount);
            i++;
        }
        sql.Append("END, ");

        // Status
        sql.Append("Status = CASE Id ");
        i = 0;
        foreach (var order in orders)
        {
            sql.Append($"WHEN @Id{i} THEN @Status{i} ");
            parameters.Add($"Status{i}", order.Status);
            i++;
        }
        sql.Append("END ");

        // WHERE
        parameters.Add("Ids", ids);
        sql.Append("WHERE Id IN @Ids");

        await _connection.ExecuteAsync(sql.ToString(), parameters);
    }
}
