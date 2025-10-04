using System.Text.Json;
using FinchServer.Beets;
using FinchServer.Database;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Configuration.AddEnvironmentVariables();
builder.Services.AddTransient<BeetsConfiguration>();

// Database

// Use singleton for performance, this is fine for read-only operations.
// SQLite allows concurrent reads, and there's no transaction state to worry about.
builder.Services.AddDbContext<BeetsContext>(ServiceLifetime.Singleton);

builder.Services.AddDbContext<DataContext>();

// https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services
    .AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    });

var app = builder.Build();

// Migrate database
using (var scope = app.Services.CreateScope()) {
    var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    await dataContext.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();