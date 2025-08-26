using RefactoringChallenge.Domain.Entities;

namespace RefactoringChallenge.Application.Interfaces.Services;

/// <summary>
/// Service working with customers
/// </summary>
public interface ICustomerService
{
    public Task<Customer?> GetByIdAsync(int customerId);
}
