using CommerceCore.Shared.Entities;

namespace CommerceCore.Domain.Entities.Identity;

/// <summary>A named role (e.g. "StoreAdmin", "CatalogManager", "SupportAgent").
/// Roles are store-scoped when StoreId is set, or global/platform when null.</summary>
public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? StoreId { get; set; }
    public bool IsSystemRole { get; set; } // system roles cannot be deleted via API

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

/// <summary>A granular capability, e.g. "products.create", "orders.refund".
/// Permissions are global definitions; Roles are granted a subset via RolePermission.</summary>
public class Permission : BaseEntity
{
    public string Code { get; set; } = string.Empty;   // e.g. "products.create"
    public string Name { get; set; } = string.Empty;    // e.g. "Create Products"
    public string? Description { get; set; }
    public string Module { get; set; } = string.Empty;  // e.g. "Catalog", "Orders", "Users"

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

/// <summary>Join entity granting a Permission to a Role.</summary>
public class RolePermission : BaseEntity
{
    public Guid RoleId { get; set; }
    public Role? Role { get; set; }

    public Guid PermissionId { get; set; }
    public Permission? Permission { get; set; }
}

/// <summary>Join entity assigning a Role to a User.</summary>
public class UserRole : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public Guid RoleId { get; set; }
    public Role? Role { get; set; }
}
