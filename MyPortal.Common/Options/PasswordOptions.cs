namespace MyPortal.Common.Options;

public class PasswordOptions
{
    public int RequiredLength { get; set; } = 8;
    public bool RequireNonAlphanumeric { get; set; } = false;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
}