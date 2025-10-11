using System.Text.Json;
using FinchServer.Metadata;
using FinchServer.Metadata.FanartTV;
using FinchServer.Beets;
using FinchServer.Database;
using FinchServer.Utilities;
using Microsoft.EntityFrameworkCore;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Configuration.AddEnvironmentVariables();
builder.Services.AddHttpClient();

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

// Metadata
builder.Services.AddTransient<IMetadataFetcher, FanartTvMetadataFetcher>();
builder.Services.AddSingleton<IMetadataManager, MetadataManager>();

// https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services
    .AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    });

var app = builder.Build();

// Apply database migrations
using (var scope = app.Services.CreateScope()) {
    var db = scope.ServiceProvider.GetRequiredService<DataContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();