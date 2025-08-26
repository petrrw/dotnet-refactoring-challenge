using RefactoringChallenge.Domain.Entities;

namespace RefactoringChallenge.Domain.Services
{
    /// <summary>
    /// Strategy for calculating discounts
    /// </summary>
    public interface IDiscountStrategy
    {
        /// <summary>
        /// Calculates discount for an order and customer
        /// </summary>
        /// <param name="order">Order for calculation</param>
        /// <param name="customer">Customer for which the discount is calculated</param>
        /// <returns>DTO with discount info</returns>
        public DiscountDTO CalculateDiscount(Order order, Customer customer);
    }
}
