using Microsoft.EntityFrameworkCore;
using Purchases.API.Application.Interfaces;
using Purchases.API.Application.Services;
using Purchases.API.Infrastructure.Integrations;
using Purchases.API.Infrastructure.Persistence;
using Purchases.API.Infrastructure.Repositories;
using Scalar.AspNetCore;
using Shared.Core.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<PurchasesDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();

var inventoryBaseUrl = builder.Configuration["ServiceUrls:InventoryAPI"] ?? "http://localhost:5026";
builder.Services.AddHttpClient<IInventoryIntegrationService, InventoryIntegrationService>(client =>
{
    client.BaseAddress = new Uri(inventoryBaseUrl.TrimEnd('/') + "/");
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Purchases API - ISW-312")
               .WithTheme(ScalarTheme.Moon)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();
