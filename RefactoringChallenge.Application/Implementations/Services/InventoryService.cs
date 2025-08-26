using RefactoringChallenge.Application.Interfaces.Data;
using RefactoringChallenge.Application.Interfaces.Services;

namespace RefactoringChallenge.Application.Implementations.Service;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;

    public InventoryService(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task DecreaseStockBulkAsync(IEnumerable<(int ProductId, int Quantity)> items)
        => await _inventoryRepository.DecreaseStockBulkAsync(items);

    public async Task<bool> AreAllInStockInRequiredQuantities(IEnumerable<(int ItemId, int RequiredQuantity)> requirements)
    {
        var itemIds = requirements.Select(x => x.ItemId);
        var stockQuantities = await _inventoryRepository.GetItemStockQuantitiesByIdsAsync(itemIds);

        foreach (var req in requirements)
        {
            if (!stockQuantities.TryGetValue(req.ItemId, out var available) || available < req.RequiredQuantity)
            {
                return false; // Either the product is not in the database or there is not enough quantity
            }
        }

        return true;
    }
}
