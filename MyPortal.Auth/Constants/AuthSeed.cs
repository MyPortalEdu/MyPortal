namespace MyPortal.Auth.Constants
{
    public class AuthSeed
    {
        public string AdminEmail { get; set; } = "admin@myportal.local";
        public string AdminPassword { get; set; } = "Passw0rd!";
        public string AdminUserName { get; set; } = "admin";
        public string[] SystemRoles { get; set; } = new[] { "System Administrator" };
        // client config
        public string PublicClientId { get; set; } = "myportal-client";
        public string PublicClientDisplayName { get; set; } = "MyPortal Public Client";
        public string[] PublicClientRedirectUris { get; set; } = Array.Empty<string>(); // set if you have SPA redirects
        public string[] PublicClientPostLogoutRedirectUris { get; set; } = Array.Empty<string>();
    }
}
