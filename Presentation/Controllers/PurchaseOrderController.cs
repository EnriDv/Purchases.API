using Microsoft.AspNetCore.Mvc;
using Purchases.API.Application.DTOs;
using Purchases.API.Application.Interfaces;
using Purchases.API.Domain.Enums;

namespace Purchases.API.Presentation.Controllers;

/// <summary>
/// PurchaseOrder — Contrato v1
/// Base: /api/purchases/companies/{companyCen}/orders
/// </summary>
[ApiController]
[Route("api/purchases/companies/{companyCen}/orders")]
public class PurchaseOrderController : ControllerBase
{
    private readonly IPurchaseOrderService _orders;

    public PurchaseOrderController(IPurchaseOrderService orders) => _orders = orders;

    [HttpGet]
    public async Task<IActionResult> GetOrders(
        string companyCen,
        [FromQuery] PurchaseStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool sortDescending = true) =>
        Ok(await _orders.GetOrdersAsync(companyCen, status, page, pageSize, sortDescending));

    [HttpPost]
    public async Task<IActionResult> CreateOrder(string companyCen, [FromBody] CreatePurchaseOrderDto request)
    {
        var result = await _orders.CreateOrderAsync(companyCen, request);
        return CreatedAtAction(nameof(GetOrder), new { companyCen, orderCen = result.OrderCen }, result);
    }

    [HttpGet("{orderCen}")]
    public async Task<IActionResult> GetOrder(string companyCen, string orderCen) =>
        Ok(await _orders.GetOrderAsync(companyCen, orderCen));

    [HttpPost("{orderCen}/confirm")]
    public async Task<IActionResult> ConfirmOrder(string companyCen, string orderCen) =>
        Ok(await _orders.ConfirmOrderAsync(companyCen, orderCen));
}
