using BackgroundTaskService;
using CustomAuthenticationApp.Context;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
// Register MS SQL Connection Context directly within the worker service environment
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString)); // Target MS SQL Server configurations

builder.Services.AddHostedService<DatabaseJobWorker>();
var host = builder.Build();
host.Run();
