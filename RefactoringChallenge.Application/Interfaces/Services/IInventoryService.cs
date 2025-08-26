namespace RefactoringChallenge.Application.Interfaces.Services;

/// <summary>
/// Service working with inventory
/// </summary>
public interface IInventoryService
{
    /// <summary>
    /// Decreases the stock quantities for multiple products in bulk.
    /// </summary>
    /// <param name="items">Items which quantity should be decreased</param>
    /// <returns>Awaitable <see cref="Task"/></returns>
    Task DecreaseStockBulkAsync(IEnumerable<(int ProductId, int Quantity)> items);

    /// <summary>
    /// Checks if all required items are in stock in required quantities
    /// </summary>
    /// <param name="requirements"></param>
    /// <returns>True if all items are in stock in required quantities, otherwise null</returns>
    Task<bool> AreAllInStockInRequiredQuantities(IEnumerable<(int ItemId, int RequiredQuantity)> requirements);
}
