using System.Reflection;
using CommerceCore.Shared.Entities;

namespace CommerceCore.Application.Common.Generic;

/// <summary>
/// Copies every simple, client-settable scalar property (string, number, bool, enum,
/// DateTime, Guid, and their nullable forms) from a posted entity onto the tracked
/// entity being updated — used by the generic Api layer's PUT endpoint, where the
/// request body is the raw entity rather than a bespoke UpdateDto (see
/// GenericCrudController's doc comment for why that trade-off is deliberate here).
/// Navigation properties, collections, and server-managed audit/concurrency fields
/// inherited from BaseEntity are deliberately excluded — those are never
/// client-settable, regardless of what the request body contains.
/// </summary>
public static class EntityPropertyCopier
{
    private static readonly HashSet<string> ExcludedProperties = new()
    {
        nameof(BaseEntity.Id),
        nameof(BaseEntity.CreatedDate),
        nameof(BaseEntity.CreatedBy),
        nameof(BaseEntity.ModifiedDate),
        nameof(BaseEntity.ModifiedBy),
        nameof(BaseEntity.DeletedDate),
        nameof(BaseEntity.DeletedBy),
        nameof(BaseEntity.IsDeleted),
        nameof(BaseEntity.Version)
    };

    public static void CopyScalarProperties<TEntity>(TEntity source, TEntity target) where TEntity : BaseEntity
    {
        foreach (var prop in typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead || !prop.CanWrite) continue;
            if (ExcludedProperties.Contains(prop.Name)) continue;
            if (!IsSimpleType(prop.PropertyType)) continue;

            var value = prop.GetValue(source);
            prop.SetValue(target, value);
        }
    }

    private static bool IsSimpleType(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        return underlying.IsPrimitive
            || underlying.IsEnum
            || underlying == typeof(string)
            || underlying == typeof(decimal)
            || underlying == typeof(DateTime)
            || underlying == typeof(DateOnly)
            || underlying == typeof(Guid);
    }
}
