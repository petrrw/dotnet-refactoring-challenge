using RefactoringChallenge.Domain.Entities;

namespace RefactoringChallenge.Application.Interfaces.Services
{
    /// <summary>
    /// Service for logging order events -> could be implemented using ILogger/ILoggerProvider..
    /// </summary>
    public interface IOrderLogService
    {
        /// <summary>
        /// Logs new <see cref="OrderLog"/>
        /// </summary>
        /// <param name="log">Log to be logged</param>
        /// <returns>Awaitable <see cref="Task"/></returns>
        Task LogAsync(OrderLog log);
    }
}
