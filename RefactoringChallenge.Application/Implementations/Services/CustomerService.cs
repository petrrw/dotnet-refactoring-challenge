using RefactoringChallenge.Application.Interfaces.Data;
using RefactoringChallenge.Application.Interfaces.Services;
using RefactoringChallenge.Domain.Entities;

namespace RefactoringChallenge.Application.Implementations.Service;

internal class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Customer?> GetByIdAsync(int customerId)
        => await _customerRepository.GetByIdAsync(customerId);
}
