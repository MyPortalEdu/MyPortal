using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Contracts.Models.People;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.People;
using MyPortal.Services.Interfaces.Providers;

namespace MyPortal.Services.People;

public class StaffComplianceService(
    IAuthorizationService authorizationService,
    ILogger<StaffComplianceService> logger,
    IStaffMemberRepository staffMemberRepository,
    IDateTimeProvider dateTimeProvider)
    : BaseService(authorizationService, logger), IStaffComplianceService
{
    private const int DefaultHorizonDays = 90;
    private const int MinHorizonDays = 7;
    private const int MaxHorizonDays = 365;

    public async Task<StaffComplianceDashboardResponse> GetDashboardAsync(int horizonDays,
        CancellationToken cancellationToken)
    {
        // The dashboard shows DBS / right-to-work / SCR across the whole school, so it is gated on
        // the most sensitive thing it exposes — the All-scope pre-employment permission.
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.ViewAllStaffPreEmploymentChecks,
            cancellationToken);

        var days = horizonDays <= 0
            ? DefaultHorizonDays
            : Math.Clamp(horizonDays, MinHorizonDays, MaxHorizonDays);

        var today = dateTimeProvider.UtcNow.Date;
        var horizon = today.AddDays(days);

        var items = await staffMemberRepository.GetComplianceItemsAsync(today, horizon, cancellationToken);

        return new StaffComplianceDashboardResponse
        {
            HorizonDays = days,
            ExpiredCount = items.Count(i => i.Kind == "Expired"),
            ExpiringSoonCount = items.Count(i => i.Kind == "ExpiringSoon"),
            MissingCount = items.Count(i => i.Kind == "Missing"),
            Items = items.ToList()
        };
    }
}
