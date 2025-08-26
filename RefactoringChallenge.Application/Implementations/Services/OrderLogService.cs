using RefactoringChallenge.Application.Interfaces.Data;
using RefactoringChallenge.Application.Interfaces.Services;
using RefactoringChallenge.Domain.Entities;

namespace RefactoringChallenge.Application.Implementations.Services
{
    internal class OrderLogService : IOrderLogService
    {
        private readonly IOrderLogRepository _orderLogRepository;

        public OrderLogService(IOrderLogRepository orderLogRepository)
            => _orderLogRepository = orderLogRepository;

        public async Task LogAsync(OrderLog log)
            => await _orderLogRepository.InsertAsync(log);
    }
}
