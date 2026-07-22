using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Identifiers;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Documents;
using MyPortal.Contracts.Models.People;
using MyPortal.Contracts.Models.People.Students;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Models;
using MyPortal.Services.Extensions;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.People;
using MyPortal.Services.Interfaces.School;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;
using QueryKit.Sql;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.People.Students;

public class StudentService(
    IAuthorizationService authorizationService,
    ILogger<StudentService> logger,
    IStudentRepository studentRepository,
    IPersonRepository personRepository,
    IPersonService personService,
    IPersonContactService personContactService,
    IPersonAddressService personAddressService,
    IPhotoService photoService,
    IEnrolmentStatusRepository enrolmentStatusRepository,
    IBoarderStatusRepository boarderStatusRepository,
    IUpnUnknownReasonRepository upnUnknownReasonRepository,
    ILocalAuthorityRepository localAuthorityRepository,
    ISchoolService schoolService,
    IValidationService validationService,
    IUnitOfWorkFactory unitOfWorkFactory)
    : BaseService(authorizationService, logger), IStudentService
{
    /// <summary>Minimum query length before <see cref="SearchPeopleAsync"/> hits the database.</summary>
    private const int MinSearchLength = 2;

    public async Task<PageResult<StudentSummaryResponse>> GetStudentsAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default)
    {
        return await studentRepository.GetStudentsAsync(filter, sort, paging, cancellationToken);
    }

    public async Task<StudentHeaderResponse> GetHeaderAsync(Guid id, CancellationToken cancellationToken)
    {
        var row = await studentRepository.GetHeaderByIdAsync(id, cancellationToken);

        if (row == null)
        {
            throw new NotFoundException("Student not found.");
        }

        return new StudentHeaderResponse
        {
            Id = row.Id,
            PersonId = row.PersonId,
            AdmissionNumber = row.AdmissionNumber,
            DisplayName = row.DisplayName,
            PreferredName = row.PreferredName,
            PhotoId = row.PhotoId,
            Status = DeriveStatus(row)
        };
    }
    
    private static StudentStatus DeriveStatus(StudentHeaderRow row)
    {
        if (row.IsDeleted) return StudentStatus.Archived;
        if (row.DateStarting is null) return StudentStatus.None;

        var today = DateTime.UtcNow.Date;

        if (row.DateStarting.Value.Date > today) return StudentStatus.Future;
        if (row.DateLeaving is { } left && left.Date < today) return StudentStatus.Leaver;
        return StudentStatus.Active;
    }

    public async Task<StudentBasicDetailsResponse> GetBasicDetailsAsync(Guid id, CancellationToken cancellationToken)
    {
        var details = await studentRepository.GetBasicDetailsByIdAsync(id, cancellationToken);

        if (details == null)
        {
            throw new NotFoundException("Student not found.");
        }

        return details;
    }

    public async Task UpdateBasicDetailsAsync(Guid id, StudentBasicDetailsUpsertRequest model,
        CancellationToken cancellationToken)
    {
        await validationService.ValidateAsync(model);

        var student = await studentRepository.GetByIdAsync(id, cancellationToken);

        if (student == null)
        {
            throw new NotFoundException("Student not found.");
        }

        // The Student row carries nothing that lives in this area (admission number is immutable;
        // enrolment/UPN belong to Registration), so only the shared Person bio is written.
        var bio = ToBio(model);

        await personService.UpdateBasicBioAsync(student.PersonId, bio, cancellationToken);
    }

    public async Task<StudentRegistrationDetailsResponse> GetRegistrationDetailsAsync(Guid id,
        CancellationToken cancellationToken)
    {
        var student = await studentRepository.GetByIdAsync(id, cancellationToken);

        if (student == null)
        {
            throw new NotFoundException("Student not found.");
        }

        var enrolmentStatuses = await enrolmentStatusRepository.GetListAsync(cancellationToken: cancellationToken);
        var boarderStatuses = await boarderStatusRepository.GetListAsync(cancellationToken: cancellationToken);
        var upnUnknownReasons = await upnUnknownReasonRepository.GetListAsync(cancellationToken: cancellationToken);

        return new StudentRegistrationDetailsResponse
        {
            Id = student.Id,
            AdmissionNumber = student.AdmissionNumber,
            EnrolmentStatusId = student.EnrolmentStatusId,
            BoarderStatusId = student.BoarderStatusId,
            DateStarting = student.DateStarting,
            Upn = student.Upn,
            FormerUpn = student.FormerUpn,
            UpnUnknownReasonId = student.UpnUnknownReasonId,
            Uln = student.Uln,
            LaChildId = student.LaChildId,
            IsPartTime = student.IsPartTime,
            EnrolmentStatuses = enrolmentStatuses.ToOrderedLookup(),
            BoarderStatuses = boarderStatuses.ToOrderedLookup(),
            UpnUnknownReasons = upnUnknownReasons.ToOrderedLookup()
        };
    }

    public async Task UpdateRegistrationDetailsAsync(Guid id, StudentRegistrationDetailsUpsertRequest model,
        CancellationToken cancellationToken)
    {
        await validationService.ValidateAsync(model);

        var student = await studentRepository.GetByIdAsync(id, cancellationToken);

        if (student == null)
        {
            throw new NotFoundException("Student not found.");
        }

        student.EnrolmentStatusId = model.EnrolmentStatusId;
        student.BoarderStatusId = model.BoarderStatusId;
        student.DateStarting = model.DateStarting;
        student.Upn = string.IsNullOrWhiteSpace(model.Upn) ? null : model.Upn.Trim();
        student.FormerUpn = string.IsNullOrWhiteSpace(model.FormerUpn) ? null : model.FormerUpn.Trim();
        student.UpnUnknownReasonId = model.UpnUnknownReasonId;
        student.Uln = string.IsNullOrWhiteSpace(model.Uln) ? null : model.Uln.Trim();
        student.LaChildId = string.IsNullOrWhiteSpace(model.LaChildId) ? null : model.LaChildId.Trim();
        student.IsPartTime = model.IsPartTime;

        await studentRepository.UpdateAsync(student, cancellationToken);
    }

    public async Task<GeneratedUpnResponse> GenerateUpnAsync(CancellationToken cancellationToken)
    {
        var school = await schoolService.GetLocalSchoolDetailsAsync(cancellationToken);

        if (school is null)
        {
            throw new ValidationException([new ValidationFailure("Upn",
                "The school's details must be configured before a UPN can be generated.")]);
        }

        if (school.LocalAuthorityId is null)
        {
            throw new ValidationException([new ValidationFailure("Upn",
                "The school's local authority must be set before a UPN can be generated.")]);
        }

        var localAuthority = await localAuthorityRepository.GetByIdAsync(school.LocalAuthorityId.Value, cancellationToken);

        if (localAuthority is null)
        {
            throw new ValidationException([new ValidationFailure("Upn",
                "The school's local authority could not be found.")]);
        }

        var year = DateTime.UtcNow.Year;
        var prefix9 = $"{localAuthority.LeaCode:D3}{school.EstablishmentNumber:D4}{year % 100:D2}";

        var serial = await studentRepository.GetMaxUpnSerialAsync(prefix9, cancellationToken) + 1;

        if (serial > 999)
        {
            throw new ValidationException([new ValidationFailure("Upn",
                "The UPN serial range for this year is exhausted; assign a UPN manually.")]);
        }

        return new GeneratedUpnResponse
        {
            Upn = Upn.Compose(localAuthority.LeaCode, school.EstablishmentNumber, year, serial)
        };
    }

    public async Task<PersonContactDetailsResponse> GetContactDetailsAsync(Guid id,
        CancellationToken cancellationToken)
    {
        var personId = await ResolvePersonIdAsync(id, cancellationToken);

        return await personContactService.GetContactDetailsAsync(personId, cancellationToken);
    }

    public async Task UpdateContactDetailsAsync(Guid id, PersonContactDetailsUpsertRequest model,
        CancellationToken cancellationToken)
    {
        var personId = await ResolvePersonIdAsync(id, cancellationToken);

        await personContactService.UpdateContactDetailsAsync(personId, model, cancellationToken);
    }

    public async Task<AddressListResponse> GetAddressesAsync(Guid id, CancellationToken cancellationToken)
    {
        var personId = await ResolvePersonIdAsync(id, cancellationToken);

        return await personAddressService.GetAddressesAsync(personId, cancellationToken);
    }

    public async Task<IReadOnlyList<AddressMatchResponse>> SearchAddressesAsync(string? query,
        CancellationToken cancellationToken)
    {
        return await personAddressService.SearchAddressesAsync(query, cancellationToken);
    }

    public async Task<Guid> AddAddressAsync(Guid id, PersonAddressUpsertRequest model,
        CancellationToken cancellationToken)
    {
        var personId = await ResolvePersonIdAsync(id, cancellationToken);

        return await personAddressService.AddAddressAsync(personId, model, cancellationToken);
    }

    public async Task UpdateAddressAsync(Guid id, Guid addressPersonId, PersonAddressUpdateRequest model,
        CancellationToken cancellationToken)
    {
        var personId = await ResolvePersonIdAsync(id, cancellationToken);

        await personAddressService.UpdateAddressAsync(personId, addressPersonId, model, cancellationToken);
    }

    public async Task RemoveAddressAsync(Guid id, Guid addressPersonId, CancellationToken cancellationToken)
    {
        var personId = await ResolvePersonIdAsync(id, cancellationToken);

        await personAddressService.RemoveAddressAsync(personId, addressPersonId, cancellationToken);
    }

    private async Task<Guid> ResolvePersonIdAsync(Guid studentId, CancellationToken cancellationToken)
    {
        var student = await studentRepository.GetByIdAsync(studentId, cancellationToken);

        if (student == null)
        {
            throw new NotFoundException("Student not found.");
        }

        return student.PersonId;
    }

    public async Task SetPhotoAsync(Guid id, Stream image, string contentType, string fileName,
        CancellationToken cancellationToken)
    {
        var student = await studentRepository.GetByIdAsync(id, cancellationToken)
                      ?? throw new NotFoundException("Student not found.");

        await photoService.SetPhotoAsync(student.PersonId, image, contentType, fileName, cancellationToken);
    }

    public async Task<DocumentContentResponse> GetPhotoAsync(Guid id, CancellationToken cancellationToken)
    {
        var student = await studentRepository.GetByIdAsync(id, cancellationToken)
                      ?? throw new NotFoundException("Student not found.");

        return await photoService.GetPhotoContentAsync(student.PersonId, cancellationToken);
    }

    public async Task DeletePhotoAsync(Guid id, CancellationToken cancellationToken)
    {
        var student = await studentRepository.GetByIdAsync(id, cancellationToken)
                      ?? throw new NotFoundException("Student not found.");

        await photoService.DeletePhotoAsync(student.PersonId, cancellationToken);
    }

    public async Task<Guid> CreateAsync(StudentBasicDetailsUpsertRequest model, CancellationToken cancellationToken)
    {
        await validationService.ValidateAsync(model);

        var studentId = SqlConvention.SequentialGuid();

        return await unitOfWorkFactory.RunInTransactionAsync<Guid>(null, async ownedUow =>
        {
            // Person row + its directory first; the student row hangs off the new person, both in
            // one transaction.
            var personId = await personService.CreateAsync(ToBio(model), cancellationToken, ownedUow);

            var admissionNumber =
                await studentRepository.GetNextAdmissionNumberAsync(cancellationToken, ownedUow.Transaction);

            var student = new Student
            {
                Id = studentId,
                PersonId = personId,
                AdmissionNumber = admissionNumber
                // All other student fields (enrolment/boarder status, UPN/ULN, SEN, FSM, etc.) stay
                // at entity defaults — populated post-creation via their area PUTs.
            };

            await studentRepository.InsertAsync(student, cancellationToken, ownedUow.Transaction);

            return studentId;
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<StudentMatchResponse>> SearchPeopleAsync(string? query,
        CancellationToken cancellationToken)
    {
        var trimmed = query?.Trim();

        if (string.IsNullOrEmpty(trimmed) || trimmed.Length < MinSearchLength)
        {
            return [];
        }

        // Escape LIKE wildcards in the user term so '%'/'_' are matched literally, then wrap as a
        // contains pattern. '[' must be escaped first.
        var escaped = trimmed.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");

        return await studentRepository.SearchPeopleForStudentCreateAsync($"%{escaped}%", cancellationToken);
    }

    public async Task<Guid> CreateForPersonAsync(StudentCreateForPersonRequest model,
        CancellationToken cancellationToken)
    {
        await validationService.ValidateAsync(model);

        var person = await personRepository.GetByIdAsync(model.PersonId, cancellationToken);

        if (person == null)
        {
            throw new NotFoundException("Person not found.");
        }

        // A person can only hold one student role — block a duplicate Student row. The FE already
        // disables already-student matches; this is the server-side guard.
        var existing = await studentRepository.GetStudentIdByPersonIdAsync(model.PersonId, cancellationToken);

        if (existing != null)
        {
            throw new ValidationException(
                [new ValidationFailure(nameof(model.PersonId), "This person is already a student.")]);
        }

        var studentId = SqlConvention.SequentialGuid();

        return await unitOfWorkFactory.RunInTransactionAsync<Guid>(null, async ownedUow =>
        {
            var admissionNumber =
                await studentRepository.GetNextAdmissionNumberAsync(cancellationToken, ownedUow.Transaction);

            // Only the Student row is created — the Person (and its bio + directory) already exists.
            var student = new Student
            {
                Id = studentId,
                PersonId = model.PersonId,
                AdmissionNumber = admissionNumber
            };

            await studentRepository.InsertAsync(student, cancellationToken, ownedUow.Transaction);

            return studentId;
        }, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var student = await studentRepository.GetByIdAsync(id, cancellationToken);

        if (student == null)
        {
            throw new NotFoundException("Student not found.");
        }

        // Soft-delete the student row only — the person may also be a contact/staff member.
        await studentRepository.DeleteAsync(id, cancellationToken);
    }

    private static PersonBasicBio ToBio(StudentBasicDetailsUpsertRequest model) => new(
        Title: model.Title,
        FirstName: model.FirstName,
        MiddleName: model.MiddleName,
        LastName: model.LastName,
        PreferredFirstName: model.PreferredFirstName,
        PreferredLastName: model.PreferredLastName,
        PhotoId: model.PhotoId,
        Gender: model.Gender,
        Dob: model.Dob,
        Deceased: model.Deceased);
}
