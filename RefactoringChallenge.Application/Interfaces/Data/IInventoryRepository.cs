namespace RefactoringChallenge.Application.Interfaces.Data;

public interface IInventoryRepository
{
    Task<int?> GetStockQuantityAsync(int productId);
    Task DecreaseStockAsync(int productId, int value);
}
