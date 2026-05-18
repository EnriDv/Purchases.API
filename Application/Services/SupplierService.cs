using Purchases.API.Application.DTOs;
using Purchases.API.Application.Interfaces;
using Shared.Core.Exceptions;

namespace Purchases.API.Application.Services;

public class SupplierService : ISupplierService
{
    private readonly IUnitOfWork _uow;

    public SupplierService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<SupplierDto>> GetSuppliersAsync(string companyCen)
    {
        var companyId = await ResolveCompanyIdAsync(companyCen);
        var suppliers = await _uow.Suppliers.GetAllAsync(s => s.CompanyId == companyId && s.Active);

        return suppliers.Select(s => new SupplierDto
        {
            SupplierCen = s.Code,
            Name = s.Name
        }).OrderBy(s => s.Name).ToList();
    }

    private async Task<int> ResolveCompanyIdAsync(string companyCen)
    {
        if (!int.TryParse(companyCen, out var id))
            throw new ValidationException($"CEN de empresa inválido: {companyCen}");

        var company = await _uow.Companies.GetByIdAsync(id);
        if (company == null || !company.Active)
            throw new NotFoundException($"Empresa no encontrada: {companyCen}");

        return id;
    }
}
