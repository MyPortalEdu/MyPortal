namespace MyPortal.Common.Constants;

/// <summary>
/// Well-known AgencyType IDs seeded by 20251101000300_seed_uk_data.sql. Used where
/// the type is fixed by domain (e.g. every School row is backed by an Agency with
/// type Educational Provider — the SPA never picks one).
/// </summary>
public static class AgencyTypes
{
    public static readonly Guid EducationalProvider = Guid.Parse("7B32B95C-082C-4DE9-8050-A6DF83F6D849");
}
