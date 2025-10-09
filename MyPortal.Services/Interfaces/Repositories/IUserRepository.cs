using MyPortal.Contracts.Models.System.Users;
using MyPortal.Core.Entities;
using MyPortal.Services.Interfaces.Repositories.Base;

namespace MyPortal.Services.Interfaces.Repositories;

public interface IUserRepository : IEntityRepository<User>
{
    Task<UserDetailsDto?> GetDetailsByIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<UserInfoDto?> GetInfoByIdAsync(Guid userId, CancellationToken cancellationToken);
}