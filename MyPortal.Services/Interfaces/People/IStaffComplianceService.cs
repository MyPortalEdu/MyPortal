using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Interfaces.People;

public interface IStaffComplianceService
{
    /// <summary>
    /// The HR compliance dashboard: expiring/expired and missing safeguarding, right-to-work,
    /// training and contract records across current/future staff. <paramref name="horizonDays"/>
    /// sets how far ahead "expiring soon" looks.
    /// </summary>
    Task<StaffComplianceDashboardResponse> GetDashboardAsync(int horizonDays,
        CancellationToken cancellationToken);
}
