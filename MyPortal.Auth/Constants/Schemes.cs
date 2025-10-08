namespace MyPortal.Auth.Constants
{
    public class Schemes
    {
        public const string SmartScheme = "Smart";
        public const string CookieScheme = "Identity.Application";
        public const string BearerScheme = OpenIddict.Validation.AspNetCore.OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    }
}
