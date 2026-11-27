var builder = DistributedApplication.CreateBuilder(args);

// Start SQL Server in Docker.
// Keep database files in a volume so container removal doesn't erase them.
var sqlServer = builder.AddSqlServer("sql")
    .WithContainerName("okolariet-sql")
    .WithDataVolume();

// Define the database and its connection-string name.
var database = sqlServer.AddDatabase("okolariet");

// Start the existing API host as a normal
// Supply the connection string and wait until the database is ready.
var api = builder.AddDotnetProject("api", "../../Host/Host.csproj")
    .WithReference(database)
    .WaitFor(database);

builder.AddDotnetProject("web", "../Web/Web.csproj")
    .WaitFor(api);

builder.Build().Run();