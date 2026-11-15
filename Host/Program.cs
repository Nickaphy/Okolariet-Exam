using Api.Controllers;
using Application;
using Infrastructure;
using Api;
using DotNetEnv;

// The Host is used to resolve one of Hexagonal Architecture's main principles, the Dependency Inversion Principle (DIP).
// The Host is responsible for creating the DI container and resolving all dependencies for the application.
// The Host is also responsible for starting the application and running it until it is stopped.
// This is because project references would create hard coupling, so creating a "Dummy service" i.e. the Host, is the best way to resolve this dilemma.

Env.TraversePath().Load();
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()                          // DI from Application
    .AddInfrastructure(builder.Configuration)  // DI from Infrastructure
    .AddAPI(builder.Configuration);            // DI from Api

builder.Services.AddControllers()
    .AddApplicationPart(typeof(StaffController).Assembly); // controllers live in Api

var app = builder.Build();

await app.Services.InitializeInfrastructureAsync(builder.Configuration);

app.UseHttpsRedirection();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();