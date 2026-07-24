using MyPortal.Data.Models;

namespace MyPortal.Data.Interfaces;

public interface ISwfCensusRepository
{
    Task<SwfCensusHeaderRow?> GetHeaderAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<SwfCensusMemberRow>> GetMembersAsync(DateTime referenceDate,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<SwfCensusAbsenceRow>> GetAbsencesAsync(DateTime absenceFrom, DateTime absenceTo,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<SwfCensusAllowanceRow>> GetAllowancesAsync(DateTime referenceDate,
        CancellationToken cancellationToken);
}
