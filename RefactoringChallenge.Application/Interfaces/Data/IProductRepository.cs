using RefactoringChallenge.Domain.Entities;

namespace RefactoringChallenge.Application.Interfaces.Data;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
}
