namespace RefactoringChallenge;

using MediatR;
using RefactoringChallenge.Application.Commands;
using RefactoringChallenge.Domain.Entities;
using System.Collections.Generic;

public class CustomerOrderProcessor
{
    private readonly ISender _mediator;

    public CustomerOrderProcessor(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Process all new orders for specific customer. Update discount and status.
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>List of processed orders</returns>
    public List<Order> ProcessCustomerOrders(int customerId)
    {
        var command = new ProcessCustomerOrdersCommand(customerId);
        var res = _mediator.Send(command).GetAwaiter().GetResult();
        return res;
    }
}
