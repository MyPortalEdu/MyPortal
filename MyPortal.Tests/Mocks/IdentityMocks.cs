using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace MyPortal.Tests.Mocks;

public static class IdentityMocks
{
    public static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
    {
        var store = new Mock<IUserStore<TUser>>();
        var optionsAccessor = new Mock<OptionsManager<IdentityOptions>>();
        var passwordHasher = new Mock<IPasswordHasher<TUser>>();
        var userValidators = new Mock<IEnumerable<IUserValidator<TUser>>>();
        var passwordValidators = new Mock<IEnumerable<IPasswordValidator<TUser>>>();
        var lookupNormalizer = new Mock<ILookupNormalizer>();
        var errorDescriber = new Mock<IdentityErrorDescriber>();
        var serviceProvider = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<UserManager<TUser>>>();
        return new Mock<UserManager<TUser>>(store.Object, optionsAccessor.Object, passwordHasher.Object,
            userValidators.Object, passwordValidators.Object, lookupNormalizer.Object, errorDescriber.Object,
            serviceProvider.Object, logger.Object);
    }
    
    public static Mock<RoleManager<TRole>> MockRoleManager<TRole>() where TRole : class
    {
        var store = new Mock<IRoleStore<TRole>>();
        var roleValidators = new Mock<IEnumerable<IRoleValidator<TRole>>>();
        var lookupNormalizer = new Mock<ILookupNormalizer>();
        var identityErrorDescriber = new Mock<IdentityErrorDescriber>();
        var logger =  new Mock<ILogger<RoleManager<TRole>>>();
        return new Mock<RoleManager<TRole>>(store.Object, roleValidators.Object, lookupNormalizer.Object,
            identityErrorDescriber.Object, logger.Object);
    }
}