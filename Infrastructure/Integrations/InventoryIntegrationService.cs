using System.Net.Http.Json;
using Purchases.API.Application.Interfaces;
using Shared.Core.Exceptions;

namespace Purchases.API.Infrastructure.Integrations;

public class InventoryIntegrationService : IInventoryIntegrationService
{
    private readonly HttpClient _http;
    private readonly ILogger<InventoryIntegrationService> _logger;

    public InventoryIntegrationService(HttpClient http, ILogger<InventoryIntegrationService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task IncreaseStockAsync(
        string companyCen,
        string warehouseCen,
        IEnumerable<(string ProductCen, int Quantity)> items,
        string reference)
    {
        var payload = new
        {
            items = items.Select(i => new
            {
                productCen = i.ProductCen,
                warehouseCen,
                quantity = i.Quantity
            }).ToList(),
            reference
        };

        var url = $"/api/inventory/companies/{companyCen}/stock/increase";
        var response = await _http.PostAsJsonAsync(url, payload);

        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync();
        _logger.LogWarning("Inventory increase stock failed: {Status} {Body}", response.StatusCode, body);

        throw new DomainException(
            $"No se pudo actualizar inventario: {(int)response.StatusCode} {response.ReasonPhrase}");
    }
}
