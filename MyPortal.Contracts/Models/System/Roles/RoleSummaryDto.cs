namespace MyPortal.Contracts.Models.System.Roles
{
    public class RoleSummaryDto
    {
        public Guid Id { get; set; }

        public string? Description { get; set; }

        public bool IsSystem { get; set; }

        public string? Name { get; set; }
    }
}
