using RefactoringChallenge.Domain.Entities;

namespace RefactoringChallenge.Application.Interfaces.Data;

/// <summary>
/// Repository for order logs.
/// </summary>
public interface IOrderLogRepository
{
    /// <summary>
    /// Inserts a new order log entry
    /// </summary>
    /// <param name="log">OrderLog to insert</param>
    /// <returns>Awaitable <see cref="Task"/></returns>
    Task InsertAsync(OrderLog log);
}
