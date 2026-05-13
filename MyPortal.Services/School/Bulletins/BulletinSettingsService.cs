using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Contracts.Models.Bulletins;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Interfaces.School;

namespace MyPortal.Services.School.Bulletins;

public class BulletinSettingsService : IBulletinSettingsService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IBulletinSettingsRepository _repository;
    private readonly ILogger<BulletinSettingsService> _logger;

    public BulletinSettingsService(IAuthorizationService authorizationService,
        IBulletinSettingsRepository repository, ILogger<BulletinSettingsService> logger)
    {
        _authorizationService = authorizationService;
        _repository = repository;
        _logger = logger;
    }

    public async Task<BulletinSettingsResponse> GetAsync(CancellationToken cancellationToken)
    {
        // Authors need to read the allowlist to build the audience picker, so read
        // access tracks ViewSchoolBulletins. Mutation stays admin-only on UpdateAsync.
        await _authorizationService.RequirePermissionAsync(Permissions.School.ViewSchoolBulletins, cancellationToken);

        var groups = await _repository.GetAllowedAudienceGroupsAsync(cancellationToken);
        return new BulletinSettingsResponse { AllowedAudienceGroups = groups };
    }

    public async Task UpdateAsync(BulletinSettingsUpdateRequest model, CancellationToken cancellationToken)
    {
        await _authorizationService.RequirePermissionAsync(Permissions.SystemAdmin.BulletinSettings, cancellationToken);

        await _repository.ReplaceAllowedAudienceGroupsAsync(model.AllowedAudienceGroupIds, cancellationToken);

        _logger.LogInformation("Bulletin settings updated: {count} allowed audience groups",
            model.AllowedAudienceGroupIds.Count);
    }
}
