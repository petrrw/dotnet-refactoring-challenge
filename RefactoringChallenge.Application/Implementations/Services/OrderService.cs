using RefactoringChallenge.Application.Interfaces.Data;
using RefactoringChallenge.Application.Interfaces.Services;
using RefactoringChallenge.Domain.Entities;
using RefactoringChallenge.Domain.Services;

namespace RefactoringChallenge.Application.Implementations.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IDiscountStrategy _discountStrategy;
    private readonly IInventoryService _inventoryService;
    private readonly IOrderLogService _orderLogService;

    public OrderService(IOrderRepository orderRepository, IDiscountStrategy discountStrategy, IInventoryService inventoryService, IOrderLogService orderLogService)
    {
        _orderRepository = orderRepository;
        _discountStrategy = discountStrategy;
        _inventoryService = inventoryService;
        _orderLogService = orderLogService;
    }

    public async Task<List<Order>> GetPendingOrdersByCustomerAsync(int customerId, bool includeItemsWithProduct)
        => await _orderRepository.GetPendingOrdersByCustomerAsync(customerId, includeItemsWithProduct);

    public Task UpdateOrdersBulkAsync(IEnumerable<Order> orders)
        => _orderRepository.UpdateOrdersBulkAsync(orders);

    public async Task<List<Order>> ProcessCustomerOrdersAsync(Customer customer)
    {
        var processedOrders = new List<Order>();
        var pendingOrders = await GetPendingOrdersByCustomerAsync(customer.Id, true);

        foreach (var order in pendingOrders)
        {
            applyDiscount(order, customer);

            var quantityRequirements = order.Items
                .Select(i => (i.ProductId, i.Quantity));

            bool allProductsAvailable = await _inventoryService
                .AreAllInStockInRequiredQuantities(quantityRequirements);

            string logMessage = string.Empty;

            if (allProductsAvailable)
            {
                await _inventoryService.DecreaseStockBulkAsync(order.Items.Select(i => (i.ProductId, i.Quantity)));

                order.Status = "Ready"; // Would be nice to use enum
                logMessage = $"Order completed with {order.DiscountPercent}% discount. Total price: {order.TotalAmount}";
            }
            else
            {
                order.Status = "OnHold";
                logMessage = "Order on hold. Some items are not on stock.";
            }


            await _orderLogService.LogAsync(new() { OrderId = order.Id, LogDate = DateTime.Now, Message = logMessage });
            processedOrders.Add(order);
        }

        await _orderRepository.UpdateOrdersBulkAsync(processedOrders);

        return processedOrders;
    }

    private void applyDiscount(Order order, Customer customer)
    {
        // This could be moved to the domain layer..
        var discount = _discountStrategy.CalculateDiscount(order, customer);
        order.DiscountPercent = discount.discountPercent;
        order.DiscountAmount = discount.discountAmount;
        order.TotalAmount = discount.finalAmount;
    }
}
