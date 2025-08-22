using RefactoringChallenge.Domain.Entities;

namespace RefactoringChallenge.Application.Interfaces.Data;

public interface IOrderLogRepository
{
    Task InsertAsync(OrderLog log);
}
