namespace MyPortal.Contracts.Interfaces.System.Roles
{
    public interface IUpsertRoleDto
    {
        string? Description { get; }

        string? Name { get; }

        IList<Guid> PermissionIds { get; }
    }
}
