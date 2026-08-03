using Asp.Versioning;
using CommerceCore.Application.Common.Interfaces;
using CommerceCore.Domain.Entities.Customers;
using CommerceCore.Domain.Entities.Identity;
using CommerceCore.Domain.Entities.Reference;
using CommerceCore.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommerceCore.Api.Controllers;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/addresses")]
public class AddressesController : ControllerBase
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _tenant;

    public AddressesController(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ICurrentTenantService tenant)
    {
        _db = db;
        _currentUser = currentUser;
        _tenant = tenant;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Address>>> GetAddresses(CancellationToken cancellationToken)
    {
        var customer = await GetOrCreateCustomerAsync(cancellationToken);
        var addresses = await _db.Addresses
            .Where(a => a.CustomerId == customer.Id)
            .OrderByDescending(a => a.IsDefaultShipping)
            .ThenBy(a => a.CreatedDate)
            .ToListAsync(cancellationToken);
        return Ok(addresses);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Address>> GetAddress(Guid id, CancellationToken cancellationToken)
    {
        var address = await FindAddressAsync(id, cancellationToken);
        return Ok(address);
    }

    [HttpPost]
    public async Task<ActionResult<Address>> CreateAddress(Address address, CancellationToken cancellationToken)
    {
        var customer = await GetOrCreateCustomerAsync(cancellationToken);

        // If country not provided, default to India (seeded Iso2Code == "IN")
        if (address.CountryId == null || address.CountryId == Guid.Empty)
            address.CountryId = await GetDefaultIndiaCountryIdAsync(cancellationToken);

        ValidateAddress(address);

        address.Id = Guid.NewGuid();
        address.CustomerId = customer.Id;
        address.Customer = null;

        if (address.IsDefaultShipping)
            await ClearDefaultShippingAsync(customer.Id, cancellationToken);

        if (!await _db.Addresses.AnyAsync(a => a.CustomerId == customer.Id, cancellationToken))
        {
            address.IsDefaultShipping = true;
        }

        _db.Addresses.Add(address);
        await _db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetAddress), new { id = address.Id, version = "1.0" }, address);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Address>> UpdateAddress(Guid id, Address request, CancellationToken cancellationToken)
    {
        var existing = await FindAddressAsync(id, cancellationToken);

        // If country not provided in the request, default to India
        if (request.CountryId == null || request.CountryId == Guid.Empty)
            existing.CountryId = await GetDefaultIndiaCountryIdAsync(cancellationToken);
        else
            existing.CountryId = request.CountryId;

        ValidateAddress(request);

        existing.FullName = request.FullName;
        existing.PhoneNumber = request.PhoneNumber;
        existing.AddressLine1 = request.AddressLine1;
        existing.AddressLine2 = request.AddressLine2;
        existing.City = request.City;
        existing.State = request.State;
        existing.PostalCode = request.PostalCode;
        existing.Type = request.Type;
        existing.IsDefaultShipping = request.IsDefaultShipping;
        existing.IsDefaultBilling = request.IsDefaultBilling;

        if (request.IsDefaultShipping)
            await ClearDefaultShippingAsync(existing.CustomerId, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return Ok(existing);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAddress(Guid id, CancellationToken cancellationToken)
    {
        var existing = await FindAddressAsync(id, cancellationToken);
        _db.Addresses.Remove(existing);
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private async Task<Address> FindAddressAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await GetOrCreateCustomerAsync(cancellationToken);
        var address = await _db.Addresses.FirstOrDefaultAsync(
            a => a.Id == id && a.CustomerId == customer.Id,
            cancellationToken);

        return address ?? throw new NotFoundException(nameof(Address), id);
    }

    private async Task<Customer> GetOrCreateCustomerAsync(CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
            throw new UnauthorizedAppException();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), userId);

        var customer = await _db.Customers.FirstOrDefaultAsync(
            c => c.UserId == user.Id && c.StoreId == _tenant.StoreId,
            cancellationToken);

        if (customer != null)
            return customer;

        customer = new Customer
        {
            StoreId = _tenant.StoreId,
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.PhoneNumber,
            IsGuest = false
        };

        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(cancellationToken);
        return customer;
    }

    private async Task ClearDefaultShippingAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var defaults = await _db.Addresses
            .Where(a => a.CustomerId == customerId && a.IsDefaultShipping)
            .ToListAsync(cancellationToken);

        foreach (var address in defaults)
        {
            address.IsDefaultShipping = false;
        }
    }

    /// <summary>
    /// Returns the seeded India country Id (Iso2Code == "IN").
    /// Throws NotFoundException if the country isn't present in the reference data.
    /// </summary>
    private async Task<Guid> GetDefaultIndiaCountryIdAsync(CancellationToken cancellationToken)
    {
        var country = await _db.Set<Country>().FirstOrDefaultAsync(c => c.Iso2Code == "IN", cancellationToken);
        if (country == null)
            throw new NotFoundException("Country", "IN");
        return country.Id;
    }

    private static void ValidateAddress(Address address)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(address.FullName))
            errors[nameof(address.FullName)] = new[] { "Full name is required." };

        if (string.IsNullOrWhiteSpace(address.PhoneNumber))
            errors[nameof(address.PhoneNumber)] = new[] { "Phone number is required." };

        if (string.IsNullOrWhiteSpace(address.AddressLine1))
            errors[nameof(address.AddressLine1)] = new[] { "Street address is required." };

        if (string.IsNullOrWhiteSpace(address.City))
            errors[nameof(address.City)] = new[] { "City is required." };

        if (string.IsNullOrWhiteSpace(address.State))
            errors[nameof(address.State)] = new[] { "State is required." };

        if (string.IsNullOrWhiteSpace(address.PostalCode))
            errors[nameof(address.PostalCode)] = new[] { "Postal code is required." };

        // Country is optional from the API; controller ensures a default when missing.

        if (errors.Count > 0)
            throw new ValidationAppException(errors);
    }
}
