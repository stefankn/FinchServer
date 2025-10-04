using System.Text.Json;
using FinchServer.Beets;
using FinchServer.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Configuration.AddEnvironmentVariables();

var beetsConfiguration = new BeetsConfiguration(builder.Configuration);
builder.Services.AddSingleton(beetsConfiguration);

// Database
builder.Services.AddDbContextPool<BeetsContext>(options => {
        options.UseSqlite($"Data Source={beetsConfiguration.DatabasePath};Cache=Shared;Pooling=True");
    },
    poolSize: 128
);

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