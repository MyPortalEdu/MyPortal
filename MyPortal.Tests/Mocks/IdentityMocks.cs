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

        // Use IOptions<IdentityOptions> instead of OptionsManager
        var optionsAccessor = new Mock<IOptions<IdentityOptions>>();
        optionsAccessor.Setup(o => o.Value).Returns(new IdentityOptions());

        var passwordHasher = new Mock<IPasswordHasher<TUser>>();
        var userValidators = new List<IUserValidator<TUser>> { new Mock<IUserValidator<TUser>>().Object };
        var passwordValidators = new List<IPasswordValidator<TUser>> { new Mock<IPasswordValidator<TUser>>().Object };
        var lookupNormalizer = new Mock<ILookupNormalizer>();
        var errorDescriber = new IdentityErrorDescriber();
        var serviceProvider = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<UserManager<TUser>>>();

        return new Mock<UserManager<TUser>>(
            store.Object,
            optionsAccessor.Object,
            passwordHasher.Object,
            userValidators,
            passwordValidators,
            lookupNormalizer.Object,
            errorDescriber,
            serviceProvider.Object,
            logger.Object
        );
    }
    
    
    public static Mock<RoleManager<TRole>> MockRoleManager<TRole>() where TRole : class
    {
        var store = new Mock<IRoleStore<TRole>>();
        var roleValidators = new List<IRoleValidator<TRole>> { new Mock<IRoleValidator<TRole>>().Object };
        var lookupNormalizer = new Mock<ILookupNormalizer>();
        var identityErrorDescriber = new IdentityErrorDescriber();
        var logger = new Mock<ILogger<RoleManager<TRole>>>();

        return new Mock<RoleManager<TRole>>(
            store.Object,
            roleValidators,
            lookupNormalizer.Object,
            identityErrorDescriber,
            logger.Object
        );
    }
}