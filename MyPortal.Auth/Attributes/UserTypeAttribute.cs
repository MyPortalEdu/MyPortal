using Microsoft.AspNetCore.Authorization;
using MyPortal.Common.Enums;

namespace MyPortal.Auth.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class UserTypeAttribute : AuthorizeAttribute
{
    private const string Prefix = "ut:"; // policy name prefix

    public UserTypeAttribute(params UserType[] allowed)
    {
        var list = (allowed ?? []).Select(x => x.ToString());
        Policy = $"{Prefix}{string.Join(",", list)}";
    }
}