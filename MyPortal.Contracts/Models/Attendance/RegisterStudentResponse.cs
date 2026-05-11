namespace MyPortal.Contracts.Models.Attendance;

public class RegisterStudentResponse
{
    public Guid StudentId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public int? AdmissionNumber { get; set; }

    // True when this student is on the register via a SessionExtraName for this
    // Session+Week, rather than the regular StudentGroupMembership. They still
    // appear on their usual register too — extras add a student to a register,
    // they don't remove them from the original.
    public bool IsExtra { get; set; }
}
