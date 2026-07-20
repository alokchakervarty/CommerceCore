using CommerceCore.Application.Common.Interfaces;
using CommerceCore.Contracts.Cart;
using MediatR;

namespace CommerceCore.Application.Features.Cart;

public record GetCartQuery : IRequest<CartResponse>;

public class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _tenant;

    public GetCartQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser, ICurrentTenantService tenant)
    {
        _db = db;
        _currentUser = currentUser;
        _tenant = tenant;
    }

    public async Task<CartResponse> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var customer = await CustomerResolver.GetOrCreateForCurrentUserAsync(_db, _currentUser, _tenant, cancellationToken);
        return await CartMapper.ToResponseAsync(_db, customer.Id, cancellationToken);
    }
}
