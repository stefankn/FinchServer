using System.Text.Json;
using FinchServer.Beets;
using FinchServer.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Configuration.AddEnvironmentVariables();

var beetsConfiguration = new BeetsConfiguration(builder.Configuration);
builder.Services.AddSingleton(beetsConfiguration);

// Beets database
builder.Services.AddDbContextPool<BeetsContext>(options => {
        options.UseSqlite($"Data Source={beetsConfiguration.DatabasePath};Cache=Shared;Pooling=True");
    },
    poolSize: 128
);

// Finch database
var contentRootPath = builder.Environment.ContentRootPath;
builder.Services.AddDbContextPool<DataContext>(options => {
        var path = Path.Combine(contentRootPath, "data", "Finch.db");
        options.UseSqlite($"Data Source={path};Cache=Shared;Pooling=True");
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();