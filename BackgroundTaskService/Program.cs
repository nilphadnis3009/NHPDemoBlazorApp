using BackgroundTaskService;
using CustomAuthenticationApp.Context;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
// Register MS SQL Connection Context directly within the worker service environment
// DB Layer & App Caches Setup
var connectionString = builder.Configuration.GetConnectionString("MysqlConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString) // Automatically detects MySQL/MariaDB version
    ));

builder.Services.AddHostedService<DatabaseJobWorker>();
var host = builder.Build();
host.Run();
