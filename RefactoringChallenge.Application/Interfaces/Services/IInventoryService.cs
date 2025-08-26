namespace RefactoringChallenge.Application.Interfaces.Services;

public interface IInventoryService
{
    Task DecreaseStockBulkAsync(IEnumerable<(int ProductId, int Quantity)> items);
    Task<bool> AreAllInStockInRequiredQuantities(IEnumerable<(int ItemId, int RequiredQuantity)> requirements);
}
