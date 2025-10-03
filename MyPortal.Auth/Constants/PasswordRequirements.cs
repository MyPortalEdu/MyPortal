namespace MyPortal.Auth.Constants;

public class PasswordRequirements
{
    public const int RequiredLength = 8;
    public const bool RequireNonAlphanumeric = false;
    public const bool RequireLowercase = true;
    public const bool RequireUppercase = true;
    public const bool RequireDigit = true;
}