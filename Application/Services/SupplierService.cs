using Purchases.API.Application.DTOs;
using Purchases.API.Application.Interfaces;
using Shared.Core.Cen;

namespace Purchases.API.Application.Services;

public class SupplierService : ISupplierService
{
    private readonly IUnitOfWork _uow;

    public SupplierService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<SupplierDto>> GetSuppliersAsync(string companyCen)
    {
        var companyId = await PurchasesCenResolver.ResolveCompanyIdAsync(_uow, companyCen);
        var suppliers = await _uow.Suppliers.GetAllAsync(s => s.CompanyId == companyId && s.Active);

        return suppliers.Select(s => new SupplierDto
        {
            SupplierCen = CenParser.Format(s.Cen),
            Name = s.Name
        }).OrderBy(s => s.Name).ToList();
    }
}
