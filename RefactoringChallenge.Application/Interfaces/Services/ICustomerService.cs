using RefactoringChallenge.Domain.Entities;

namespace RefactoringChallenge.Application.Interfaces.Services;

public interface ICustomerService
{
    public Task<Customer?> GetByIdAsync(int customerId);
}
