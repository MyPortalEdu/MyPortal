namespace MyPortal.Contracts.Models.People;

public class IncrementPreviewRequest
{
    public DateTime EffectiveFrom { get; set; }
}

public class IncrementApplyRequest
{
    public DateTime EffectiveFrom { get; set; }

    /// <summary>The contracts to increment — a subset of the preview, so individuals can be excluded.</summary>
    public List<Guid> ContractIds { get; set; } = [];
}

/// <summary>
/// One staff member's move in the annual increment. <see cref="AtMaximum"/> rows are shown for
/// transparency but can't be applied — they've reached the top of their grade.
/// </summary>
public class IncrementItem
{
    public Guid ContractId { get; set; }
    public Guid StaffMemberId { get; set; }
    public string StaffName { get; set; } = null!;
    public string StaffCode { get; set; } = null!;

    public string ScaleCode { get; set; } = null!;
    public string CurrentPointCode { get; set; } = null!;
    public decimal CurrentPointValue { get; set; }
    public decimal? CurrentSalary { get; set; }

    public Guid? NextPointId { get; set; }
    public string? NextPointCode { get; set; }
    public decimal? NextPointValue { get; set; }
    public decimal? NewSalary { get; set; }

    /// <summary>Already on the grade's top point — nothing to move to.</summary>
    public bool AtMaximum { get; set; }

    /// <summary>No statutory rate on file for the next point in the school's zone; salary can't be set.</summary>
    public bool MissingRate { get; set; }

    /// <summary>Already incremented for this effective date — skipped so a cycle can't apply twice.</summary>
    public bool AlreadyIncremented { get; set; }
}

public class IncrementScheduleRequest
{
    public DateTime EffectiveFrom { get; set; }
}

public class ScheduledIncrementResponse
{
    public Guid Id { get; set; }
    public Guid ServiceTermId { get; set; }
    public string ServiceTermCode { get; set; } = null!;
    public string ServiceTermDescription { get; set; } = null!;
    public DateTime EffectiveDate { get; set; }
    public string Status { get; set; } = null!;
    public DateTime? CompletedAt { get; set; }
    public int? AppliedCount { get; set; }
    public string? ScheduledBy { get; set; }

    /// <summary>Effective on or before today and still pending — ready to run.</summary>
    public bool IsDue { get; set; }
}

public class IncrementPreviewResponse
{
    public Guid ServiceTermId { get; set; }
    public DateTime EffectiveFrom { get; set; }

    /// <summary>How many rows can actually be applied (not at maximum, next point priced).</summary>
    public int EligibleCount { get; set; }

    public List<IncrementItem> Items { get; set; } = [];
}
