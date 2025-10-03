using MyPortal.Contracts.Models.Users;
using MyPortal.Core.Entities;
using MyPortal.Services.Interfaces.Repositories.Base;

namespace MyPortal.Services.Interfaces.Repositories;

public interface IUserRepository : IEntityRepository<User>
{
    Task<UserDetailsDto?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken);
}