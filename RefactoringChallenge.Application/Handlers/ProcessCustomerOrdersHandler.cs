using MediatR;
using RefactoringChallenge.Application.Commands;
using RefactoringChallenge.Application.Interfaces.Data;
using RefactoringChallenge.Domain.Entities;

namespace RefactoringChallenge.Application.Handlers
{
    public class ProcessCustomerOrdersHandler : IRequestHandler<ProcessCustomerOrdersCommand, List<Order>>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IProductRepository _productRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IOrderLogRepository _orderLogRepository;

        public ProcessCustomerOrdersHandler(
             ICustomerRepository customerRepository,
             IOrderRepository orderRepository,
             IOrderItemRepository orderItemRepository,
             IProductRepository productRepository,
             IInventoryRepository inventoryRepository,
             IOrderLogRepository orderLogRepository

            )
        {
            _customerRepository = customerRepository;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
            _inventoryRepository = inventoryRepository;
            _orderLogRepository = orderLogRepository;
        }

        public async Task<List<Order>> Handle(ProcessCustomerOrdersCommand request, CancellationToken cancellationToken)
        {
            if (request.customerId <= 0)
                throw new ArgumentException("ID zákazníka musí být kladné číslo.", nameof(request.customerId));

            var processedOrders = new List<Order>();

            // Get customer by ID
            var customer = await _customerRepository.GetByIdAsync(request.customerId);

            if (customer == null)
                throw new Exception($"Zákazník s ID {request.customerId} nebyl nalezen.");

            // Get customer's pending orders
            var pendingOrders = await _orderRepository.GetPendingOrdersByCustomerAsync(request.customerId);

            foreach (var order in pendingOrders)
            {
                // TODO: Tohle není optimální
                order.Items = await _orderItemRepository.GetByOrderIdAsync(order.Id);

                foreach(var item in order.Items)
                {
                    item.Product = await _productRepository.GetByIdAsync(item.ProductId) ?? throw new Exception($"Product with id {item.Id} not found");
                }
            }

            // TODO: 2x se prochází pending orders
            foreach (var order in pendingOrders)
            {
                decimal totalAmount = 0;

                // TODO: refaktor
                foreach (var item in order.Items)
                {
                    var subtotal = item.Quantity * item.UnitPrice;
                    totalAmount += subtotal;
                }

                decimal discountPercent = 0;

                if (customer.IsVip)
                {
                    discountPercent += 10;
                }

                int yearsAsCustomer = DateTime.Now.Year - customer.RegistrationDate.Year;
                if (yearsAsCustomer >= 5)
                {
                    discountPercent += 5;
                }
                else if (yearsAsCustomer >= 2)
                {
                    discountPercent += 2;
                }

                if (totalAmount > 10000)
                {
                    discountPercent += 15;
                }
                else if (totalAmount > 5000)
                {
                    discountPercent += 10;
                }
                else if (totalAmount > 1000)
                {
                    discountPercent += 5;
                }

                if (discountPercent > 25)
                {
                    discountPercent = 25;
                }

                decimal discountAmount = totalAmount * (discountPercent / 100);
                decimal finalAmount = totalAmount - discountAmount;

                order.DiscountPercent = discountPercent;
                order.DiscountAmount = discountAmount;
                order.TotalAmount = finalAmount;
                order.Status = "Processed";

                await _orderRepository.UpdateOrderAsync(order);

                bool allProductsAvailable = true;
                foreach (var item in order.Items)
                {

                    var stockQuantity = await _inventoryRepository.GetStockQuantityAsync(item.ProductId);
                        if (stockQuantity == null || stockQuantity < item.Quantity)
                        {
                            allProductsAvailable = false;
                            break;
                        }
                }

                if (allProductsAvailable)
                {
                    foreach (var item in order.Items)
                    {
                        await _inventoryRepository.DecreaseStockAsync(item.ProductId, item.Quantity);
                    }

                    order.Status = "Ready";
                    await _orderRepository.UpdateOrderAsync(order);

                    await _orderLogRepository.InsertAsync(new() { OrderId = order.Id, LogDate = DateTime.Now, Message = $"Order completed with {order.DiscountPercent}% discount. Total price: {order.TotalAmount}" });
                }
                else
                {
                    order.Status = "OnHold";

                    await _orderRepository.UpdateOrderAsync(order);
                    await _orderLogRepository.InsertAsync(new() { OrderId = order.Id, LogDate = DateTime.Now, Message = "Order on hold. Some items are not on stock." });
                }
                processedOrders.Add(order);
            }

            return processedOrders;
        }
    }
}
