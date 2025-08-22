using MediatR;
using RefactoringChallenge.Domain.Entities;

namespace RefactoringChallenge.Application.Commands;

public record ProcessCustomerOrdersCommand(int customerId) : IRequest<List<Order>>;
