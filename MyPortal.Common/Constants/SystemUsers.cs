namespace MyPortal.Common.Constants;

/// <summary>
/// Well-known users created by the migration runner before any seed migration that
/// needs ownership of system rows. Lets the seed migrations be self-sufficient on
/// a fresh database — no need to run AuthSeeder first to bootstrap the chicken-and-egg.
///
/// <para>
/// <b>Sentinel system user</b>: fixed-Guid, <c>IsSystem = 1</c>, <c>IsEnabled = 0</c>,
/// no password. Cannot log in. Exists purely so seed migrations have a real
/// <c>dbo.Users.Id</c> to reference for <c>CreatedById</c> / <c>LastModifiedById</c>
/// audit columns and ownership of seed rows (e.g. system bulletin categories).
/// </para>
/// <para>
/// Distinct from the admin user that <c>AuthSeeder</c> creates on first WebApi
/// boot — that's a real human account. Both currently carry <c>IsSystem = 1</c>;
/// migrations that need a system owner should reference <see cref="SentinelUserId"/>
/// directly (not <c>WHERE IsSystem = 1</c> with an ordering lookup) to be
/// deterministic.
/// </para>
/// </summary>
public static class SystemUsers
{
    /// <summary>
    /// Fixed Guid for the sentinel system user. Seeded by
    /// <c>20251101000050_seed_system_user.sql</c>. Migrations that need a
    /// non-null <c>CreatedById</c> for system-owned rows should reference this
    /// directly so they don't depend on AuthSeeder having run.
    /// </summary>
    public static readonly Guid SentinelUserId =
        new("00000000-0000-0000-0000-000000000001");
}
