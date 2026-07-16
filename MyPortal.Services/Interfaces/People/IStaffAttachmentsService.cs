using MyPortal.Core.Entities;
using MyPortal.Services.Interfaces.Documents;

namespace MyPortal.Services.Interfaces.People;

/// <summary>
/// Staff-scoped attachments façade. Documents physically hang off the staff member's
/// <see cref="Person"/> directory tree, but access is gated on the staff <c>Documents</c>
/// permission domain (relationship-scoped). A distinct interface (rather than registering the
/// open <see cref="IDirectoryEntityService{Person}"/> slot) keeps room for future
/// student/contact attachment services over the same Person tree — mirroring the
/// StaffContact/StaffAddress subtype-wrapper pattern.
/// </summary>
public interface IStaffAttachmentsService : IDirectoryEntityService<Person>;
