using CommerceCore.Application.Common.Interfaces;

namespace CommerceCore.Infrastructure.Services;

/// <summary>Thin wrapper over DateTime.UtcNow so Application handlers never call it
/// directly — keeps time fully mockable in unit tests.</summary>
public class DateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
}
