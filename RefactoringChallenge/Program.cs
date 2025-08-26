using RefactoringChallenge.Application.Extensions;
using RefactoringChallenge.Infrastructure.Extensions;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddLogging();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new Exception("Connection string not found"));

var host = builder.Build();

host.Run();
