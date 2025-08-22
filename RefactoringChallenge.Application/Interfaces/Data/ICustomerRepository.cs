using RefactoringChallenge.Domain.Entities;

namespace RefactoringChallenge.Application.Interfaces.Data;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(int customerId);
}
