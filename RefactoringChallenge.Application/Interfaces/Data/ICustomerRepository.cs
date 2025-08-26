using RefactoringChallenge.Domain.Entities;

namespace RefactoringChallenge.Application.Interfaces.Data;

/// <summary>
/// Repository for <see cref="Customer"/>
/// </summary>
public interface ICustomerRepository
{
    /// <summary>
    /// Get customer by id
    /// </summary>
    /// <param name="customerId">CustomerID</param>
    /// <returns>Customer if found, otherwise null</returns>
    Task<Customer?> GetByIdAsync(int customerId);
}
