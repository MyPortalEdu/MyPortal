namespace MyPortal.Common.Constants;

/// <summary>
/// Well-known roles seeded by 20260717000100_seed_default_roles.sql. Student and Parent carry fixed
/// ids because UserService auto-assigns them by id. System Administrator is resolved by name — its id
/// differs on databases created before role seeding was unified into a migration, so it is not fixed.
/// </summary>
public static class SystemRoles
{
    public static readonly Guid StudentRoleId = new("5EED0001-0000-4000-8000-000000000001");
    public static readonly Guid ParentRoleId = new("5EED0001-0000-4000-8000-000000000002");

    public const string SystemAdministratorRoleName = "System Administrator";
}
