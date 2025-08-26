using RefactoringChallenge.Domain.Entities;

namespace RefactoringChallenge.Domain.Services;

public class DefaultDiscountStrategy : IDiscountStrategy
{
    public DiscountDTO CalculateDiscount(Order order, Customer customer)
    {
        // Total amount of the order
        decimal totalAmount = order.Items.Sum(item => item.Quantity * item.UnitPrice);

        decimal discountPercent = 0;

        // Apply VIP discount
        if (customer.IsVip)
            discountPercent += 10;

        // Apply discount based on customer loyalty
        int yearsAsCustomer = DateTime.Now.Year - customer.RegistrationDate.Year;

        if (yearsAsCustomer >= 5)
            discountPercent += 5;
        else if (yearsAsCustomer >= 2)
            discountPercent += 2;

        // Apply discount based on order total
        discountPercent += totalAmount switch
        {
            > 10000 => 15,
            > 5000 => 10,
            > 1000 => 5,
            _ => 0
        };

        // Ensure discount does not exceed maximum allowed
        discountPercent = Math.Min(discountPercent, 25);

        // Calculate discount amount and final total
        decimal discountAmount = totalAmount * (discountPercent / 100);
        decimal finalAmount = totalAmount - discountAmount;


        return new(discountPercent, discountAmount, finalAmount);
    }
}
