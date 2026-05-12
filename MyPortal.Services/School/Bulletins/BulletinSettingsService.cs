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
        await _authorizationService.RequirePermissionAsync(Permissions.SystemAdmin.BulletinSettings, cancellationToken);

        var groups = await _repository.GetAllowedAudienceGroupsAsync(cancellationToken);
        return new BulletinSettingsResponse { AllowedAudienceGroups = groups };
    }

    public async Task UpdateAsync(BulletinSettingsUpdateRequest model, CancellationToken cancellationToken)
    {
        // Pin permission gates settings — pinners are the admins, and bulletin settings
        // are an admin-tier concern. If/when a dedicated ManageBulletinSettings perm
        // exists, swap it in here.
        await _authorizationService.RequirePermissionAsync(Permissions.SystemAdmin.BulletinSettings, cancellationToken);

        await _repository.ReplaceAllowedAudienceGroupsAsync(model.AllowedAudienceGroupIds, cancellationToken);

        _logger.LogInformation("Bulletin settings updated: {count} allowed audience groups",
            model.AllowedAudienceGroupIds.Count);
    }
}
