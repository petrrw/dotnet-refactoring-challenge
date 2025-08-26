namespace RefactoringChallenge;

using System.Data.SqlClient;
using MediatR;
using Microsoft.Data.SqlClient;
using Moq;
using NUnit.Framework;
using RefactoringChallenge.Application.Extensions;
using RefactoringChallenge.Application.Implementations.Services;
using RefactoringChallenge.Application.Interfaces.Data;
using RefactoringChallenge.Application.Interfaces.Services;
using RefactoringChallenge.Domain.Entities;
using RefactoringChallenge.Domain.Services;
using RefactoringChallenge.Infrastructure.Extensions;

[TestFixture]
public class CustomerOrderProcessorTests
{
    private readonly string _connectionString = "Server=localhost,1433;Database=refactoringchallenge;User ID=sa;Password=RCPassword1!;TrustServerCertificate=True;";
    private ISender _mediator = null!;

    [SetUp]
    public void SetUp()
    {
        SetupDatabase();

        IServiceCollection services = new ServiceCollection();

        services.AddLogging();
        services.AddApplication();
        services.AddInfrastructure(_connectionString);
        var provider = services.BuildServiceProvider();
        _mediator = provider.GetRequiredService<ISender>();
    }

    [Test]
    public void ProcessCustomerOrders_ForVipCustomerWithLargeOrder_AppliesCorrectDiscounts()
    {
        int customerId = 1; // VIP customer
        var processor = new CustomerOrderProcessor(_mediator);

        var result = processor.ProcessCustomerOrders(customerId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));

        var largeOrder = result.Find(o => o.Id == 1);
        Assert.That(largeOrder, Is.Not.Null);
        Assert.That(largeOrder!.DiscountPercent, Is.EqualTo(25)); // Max. discount 25%
        Assert.That(largeOrder.Status, Is.EqualTo("Ready"));

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqlCommand("SELECT StockQuantity FROM Inventory WHERE ProductId = 1", connection))
            {
                var newStock = (int)command.ExecuteScalar();
                Assert.That(newStock, Is.EqualTo(90)); // Origin qty 100, ordered 10
            }
        }
    }

    [Test]
    public void ProcessCustomerOrders_ForRegularCustomerWithSmallOrder_AppliesMinimalDiscount()
    {
        int customerId = 2; // Regular customer
        var processor = new CustomerOrderProcessor(_mediator);

        var result = processor.ProcessCustomerOrders(customerId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1));

        var smallOrder = result[0];
        Assert.That(smallOrder.DiscountPercent, Is.EqualTo(2)); // 2% loyalty discount
        Assert.That(smallOrder.Status, Is.EqualTo("Ready"));
    }

    [Test]
    public void ProcessCustomerOrders_ForOrderWithUnavailableProducts_SetsOrderOnHold()
    {
        int customerId = 3; // Customer with order with non-available items
        var processor = new CustomerOrderProcessor(_mediator);

        var result = processor.ProcessCustomerOrders(customerId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1));

        var onHoldOrder = result[0];
        Assert.That(onHoldOrder.Status, Is.EqualTo("OnHold"));

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqlCommand("SELECT Message FROM OrderLogs WHERE OrderId = @OrderId ORDER BY LogDate DESC", connection))
            {
                command.Parameters.AddWithValue("@OrderId", onHoldOrder.Id);
                var message = (string)command.ExecuteScalar();
                Assert.That(message, Is.EqualTo("Order on hold. Some items are not on stock."));
            }
        }
    }

    private void SetupDatabase()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            ExecuteNonQuery(connection, "DELETE FROM OrderLogs");
            ExecuteNonQuery(connection, "DELETE FROM OrderItems");
            ExecuteNonQuery(connection, "DELETE FROM Orders");
            ExecuteNonQuery(connection, "DELETE FROM Inventory");
            ExecuteNonQuery(connection, "DELETE FROM Products");
            ExecuteNonQuery(connection, "DELETE FROM Customers");

            ExecuteNonQuery(connection, @"
                INSERT INTO Customers (Id, Name, Email, IsVip, RegistrationDate) VALUES 
                (1, 'Joe Doe', 'joe.doe@example.com', 1, '2015-01-01'),
                (2, 'John Smith', 'john@example.com', 0, '2023-03-15'),
                (3, 'James Miller', 'miller@example.com', 0, '2024-01-01')
            ");

            ExecuteNonQuery(connection, @"
                INSERT INTO Products (Id, Name, Category, Price) VALUES 
                (1, 'White', 'T-Shirts', 25000),
                (2, 'Gray', 'T-Shirts', 800),
                (3, 'Gold', 'T-Shirts', 5000),
                (4, 'Black', 'T-Shirts', 500)
            ");

            ExecuteNonQuery(connection, @"
                INSERT INTO Inventory (ProductId, StockQuantity) VALUES 
                (1, 100),
                (2, 200),
                (3, 5),
                (4, 50)
            ");

            ExecuteNonQuery(connection, @"
                INSERT INTO Orders (Id, CustomerId, OrderDate, TotalAmount, Status) VALUES 
                (1, 1, '2025-04-10', 0, 'Pending'),
                (2, 1, '2025-04-12', 0, 'Pending'),
                (3, 2, '2025-04-13', 0, 'Pending'),
                (4, 3, '2025-04-14', 0, 'Pending')
            ");

            ExecuteNonQuery(connection, @"
                INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice) VALUES 
                (1, 1, 10, 25000),
                (1, 2, 5, 800),
                (2, 4, 3, 500),
                (3, 2, 1, 800),
                (4, 3, 10, 5000)
            ");
        }
    }

    private void ExecuteNonQuery(SqlConnection connection, string commandText)
    {
        using (var command = new SqlCommand(commandText, connection))
        {
            command.ExecuteNonQuery();
        }
    }

    [TestCase(true, 5, 10001, 25)]   // VIP, >= 5 years, >10000 -> 10+5+15=30% -> capped at 25%
    [TestCase(true, 4, 10000, 22)]   // VIP, 4 years, >5000 -> 10+2+10=22% -> not capped
    [TestCase(true, 2, 5001, 22)]    // VIP, 2 years, >5000 -> 10+2+10=22% -> not capped
    [TestCase(true, 2, 5000, 17)]    // VIP, 2 years, >1000 -> 10+2+5=17% -> not capped
    [TestCase(true, 2, 1001, 17)]    // VIP, 2 years, >1000 -> 10+2+5=17% -> not capped
    [TestCase(true, 2, 1000, 12)]    // VIP, 2 years, > 0 -> 10+2+0=12% -> not capped
    [TestCase(true, 1, 0, 10)]       // VIP, 1 year, 0 = 0 -> 10+0+0=0% -> not capped
    [TestCase(false, 5, 10001, 20)]   // VIP, >= 5 years, >10000 -> 0+5+15=20% -> not capped
    [TestCase(false, 4, 10000, 12)]   // VIP, 4 years, >5000 -> 0+2+10=12% -> not capped
    [TestCase(false, 2, 5001, 12)]    // VIP, 2 years, >5000 -> 0+2+10=12% -> not capped
    [TestCase(false, 2, 5000, 7)]    // VIP, 2 years, >1000 -> 0+2+5=7% -> not capped
    [TestCase(false, 2, 1001, 7)]    // VIP, 2 years, >1000 -> 0+2+5=7% -> not capped
    [TestCase(false, 2, 1000, 2)]    // VIP, 2 years, > 0 -> 0+2+0=2% -> not capped
    [TestCase(false, 1, 0, 0)]       // VIP, 1 year, 0 = 0 -> 0+0+0=0% -> not capped
    public void CalculateDiscount_DefaultStrategy_AllVariants_Test(
        bool isVip, int yearsAsCustomer, decimal itemPrice, decimal expectedDiscountPercent)
    {
        // Arrange
        var order = new Order
        {
            Items = new List<OrderItem>
            {
                new OrderItem { Quantity = 1, UnitPrice = itemPrice }
            }
        };

        var customer = new Customer
        {
            IsVip = isVip,
            RegistrationDate = DateTime.Now.AddYears(-yearsAsCustomer)
        };

        // Act
        var discount = new DefaultDiscountStrategy().CalculateDiscount(order, customer);

        // Assert
        Assert.That(discount.discountPercent, Is.EqualTo(expectedDiscountPercent));
        Assert.That(discount.discountAmount, Is.EqualTo(itemPrice * (discount.discountPercent / 100)));
        Assert.That(discount.finalAmount, Is.EqualTo(itemPrice - discount.discountAmount));
    }

    [Test]
    public void CalculateDiscount_DefaultStrategy_CountsAllItems_Test()
    {
        // Arrange
        var order = new Order
        {
            Items = new List<OrderItem>
            {
                new OrderItem { Quantity = 500, UnitPrice = 10 },
                new OrderItem { Quantity = 1000, UnitPrice = 5 }
            }
        };

        var customer = new Customer
        {
            IsVip = false,
            RegistrationDate = DateTime.Now.AddYears(0)
        };

        // Act
        var discount = new DefaultDiscountStrategy().CalculateDiscount(order, customer);

        // Assert
        Assert.That(10m, Is.EqualTo(discount.discountPercent));
        Assert.That(1000m, Is.EqualTo(discount.discountAmount));
        Assert.That(9000m, Is.EqualTo(discount.finalAmount));
    }

    [Test]
    public async Task OrderService_ProcessCustomerOrders()
    {
        // Arrange
        #region arrange
        var orderRepositoryMock = new Mock<IOrderRepository>();
        orderRepositoryMock
            .Setup(r => r.GetPendingOrdersByCustomerAsync(It.IsAny<int>(), It.IsAny<bool>()))
            .ReturnsAsync(new List<Order>()
            {
               new Order()
               {
                   Id = 1,
                   CustomerId = 1,
                   Status = "TestPending",
                   OrderDate = DateTime.Now,
                   Items = new List<OrderItem>()
                   {
                       new() {Id = 1, OrderId = 1, ProductId = 1, Quantity = 10, UnitPrice = 250, Product = new Product() { Id = 1, Name = "WhiteShirt", Price = 2500, Category = "T-Shirts" } },
                   }
               },
               new Order()
               {
                   Id = 2,
                   CustomerId = 1,
                   Status = "TestPending",
                   OrderDate = DateTime.Now,
                   Items = new List<OrderItem>()
                   {
                       new() {Id = 2, OrderId = 2, ProductId = 2, Quantity = 50, UnitPrice = 250, Product = new Product() { Id = 1, Name = "BlackShirt", Price = 2500, Category = "T-Shirts" } },
                   }
               }
            });

        var inventoryServiceMock = new Mock<IInventoryService>();
        inventoryServiceMock
            .Setup(s => s.AreAllInStockInRequiredQuantities(It.IsAny<IEnumerable<(int ItemId, int RequiredQuantity)>>()))
                 .ReturnsAsync((IEnumerable<(int ItemId, int RequiredQuantity)> reqs) =>
                 {
                     if (reqs.Any(r => r.ItemId == 2))
                         return false;

                     return true;
                 });

        var orderLogService = new Mock<IOrderLogService>();

        var discountStrategyMock = new Mock<IDiscountStrategy>();
        discountStrategyMock.Setup(discountStrategy => discountStrategy.CalculateDiscount(It.IsAny<Order>(), It.IsAny<Customer>()))
            .Returns((Order order, Customer customer) =>
            {
                if (order.Id == 1)
                    return new Domain.DiscountDTO(10, 200, 400);

                return new Domain.DiscountDTO(15, 150, 500);
            });

        var customer = new Customer() { Id = 1, IsVip = true, RegistrationDate = DateTime.Now.AddYears(-3) };
        var expectedProcessedOrders = new List<Order>()
        {
             new Order()
               {
                   Id = 1,
                   CustomerId = 1,
                   Status = "Ready",
                   OrderDate = DateTime.Now,
                   Items = new List<OrderItem>()
                   {
                       new() {Id = 1, OrderId = 1, ProductId = 1, Quantity = 10, UnitPrice = 250, Product = new Product() { Id = 1, Name = "WhiteShirt", Price = 2500, Category = "T-Shirts" } },
                   },
                   DiscountPercent = 10,
                   DiscountAmount = 200,
                   TotalAmount = 400
               },
               new Order()
               {
                   Id = 2,
                   CustomerId = 1,
                   Status = "OnHold",
                   OrderDate = DateTime.Now,
                   Items = new List<OrderItem>()
                   {
                       new() {Id = 2, OrderId = 2, ProductId = 2, Quantity = 50, UnitPrice = 250, Product = new Product() { Id = 1, Name = "BlackShirt", Price = 2500, Category = "T-Shirts" } },
                   },
                   DiscountPercent = 15,
                   DiscountAmount = 150,
                   TotalAmount = 500
               }
        };

        IOrderService orderService = new OrderService(orderRepositoryMock.Object, discountStrategyMock.Object, inventoryServiceMock.Object, orderLogService.Object);
        #endregion

        // Act
        var actualProcessedOrders = await orderService.ProcessCustomerOrdersAsync(customer);

        // Assert 

        // Stock decrease for order with all items available
        inventoryServiceMock.Verify(s =>
            s.DecreaseStockBulkAsync(
                It.Is<IEnumerable<(int ItemId, int RequiredQuantity)>>(reqs =>
                    reqs.All(r => r.ItemId == 1 && r.RequiredQuantity == 10)
                )),
            Times.Once);

        // No stock decrease for order with unavailable items
        inventoryServiceMock.Verify(s =>
            s.DecreaseStockBulkAsync(
                It.Is<IEnumerable<(int ItemId, int RequiredQuantity)>>(reqs =>
                    reqs.Any(r => r.ItemId != 1)
                )),
            Times.Never);

        // All orders should be updated
        orderRepositoryMock.Verify(s =>
            s.UpdateOrdersBulkAsync(
                It.Is<IEnumerable<Order>>(reqs =>
                    reqs.Where(o => o.Id == 1).All(o => o.Status == "Ready" && o.DiscountPercent == 10 ) &&
                    reqs.Where(o => o.Id == 2).All(o => o.Status == "OnHold" && o.DiscountPercent == 15)
                )),
            Times.Once);

        // Verify logging
        orderLogService.Verify(s =>
                  s.LogAsync(
                      It.Is<OrderLog>(reqs =>
                      reqs.Message == "Order on hold. Some items are not on stock." &&
                      reqs.OrderId == 2
                      )),
                  Times.Once);

        orderLogService.Verify(s =>
          s.LogAsync(
              It.Is<OrderLog>(reqs =>
              reqs.Message == $"Order completed with 10% discount. Total price: 400" &&
              reqs.OrderId == 1
              )),
          Times.Once);

        // Verify actual orders match expected orders (discount applied..)
        Assert.That(actualProcessedOrders, Is.EqualTo(expectedProcessedOrders)
         .Using<Order>((x, y) =>
             x.Id == y.Id &&
             x.CustomerId == y.CustomerId &&
             x.Status == y.Status &&
             x.DiscountPercent == y.DiscountPercent &&
             x.DiscountAmount == y.DiscountAmount &&
             x.TotalAmount == y.TotalAmount &&
             x.Items.Count == y.Items.Count &&
             x.Items.Zip(y.Items, (xi, yi) =>
                 xi.Id == yi.Id &&
                 xi.ProductId == yi.ProductId &&
                 xi.Quantity == yi.Quantity &&
                 xi.UnitPrice == yi.UnitPrice
             ).All(b => b)
         ));
    }
}
