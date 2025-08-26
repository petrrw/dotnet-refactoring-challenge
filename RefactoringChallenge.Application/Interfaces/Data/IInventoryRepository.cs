namespace RefactoringChallenge.Application.Interfaces.Data;

public record StockRequirement(int ProductId, int RequiredQuantity);

/// <summary>
/// Repository for managing inventory
/// </summary>
public interface IInventoryRepository
{
    /// <summary>
    /// Retrieves the stock quantities for the specified product IDs
    /// </summary>
    /// <param name="ids">The collection of product IDs to look up</param>
    /// <returns>
    /// A <see cref="Dictionary{TKey, TValue}"/> where the key is the product ID 
    /// and the value is the corresponding quantity available in stock
    /// </returns>
    Task<Dictionary<int, int>> GetItemStockQuantitiesByIdsAsync(IEnumerable<int> ids);

    /// <summary>
    /// Decreases the stock quantities for multiple products in bulk.
    /// </summary>
    /// <param name="items">
    /// A collection of tuples where each tuple contains the product ID and the quantity to subtract from stock
    /// </param>
    /// <returns>Awaitable <see cref="Task"/></returns>
    Task DecreaseStockBulkAsync(IEnumerable<(int ProductId, int Quantity)> items);
}
