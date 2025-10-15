using MyPortal.Common.Enums;

namespace MyPortal.Contracts.Models.System.Users
{
    public class UserInfoDto
    {
        public Guid Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public UserType UserType { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsSystem { get; set; }
        public string DisplayName { get; set; }
        public string[] Permissions { get; set; }
    }
}
