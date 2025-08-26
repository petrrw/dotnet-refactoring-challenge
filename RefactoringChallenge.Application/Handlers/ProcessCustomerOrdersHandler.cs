using MediatR;
using RefactoringChallenge.Application.Commands;
using RefactoringChallenge.Application.Interfaces.Services;
using RefactoringChallenge.Domain.Entities;

namespace RefactoringChallenge.Application.Handlers
{
    public class ProcessCustomerOrdersHandler : IRequestHandler<ProcessCustomerOrdersCommand, List<Order>>
    {
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;

        public ProcessCustomerOrdersHandler(ICustomerService customerService, IOrderService orderService)
        {
            _customerService = customerService;
            _orderService = orderService;
        }

        public async Task<List<Order>> Handle(ProcessCustomerOrdersCommand request, CancellationToken cancellationToken)
        {
            if (request.customerId <= 0)
                throw new ArgumentException("ID zákazníka musí být kladné číslo.", nameof(request.customerId));

            var customer = await _customerService.GetByIdAsync(request.customerId);

            if (customer == null)
                throw new Exception($"Zákazník s ID {request.customerId} nebyl nalezen.");

            return await _orderService.ProcessCustomerOrdersAsync(customer);
        }
    }
}
