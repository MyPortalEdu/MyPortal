using System.Globalization;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Contracts.Models.People;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Models;
using MyPortal.Services.Interfaces.People;
using MyPortal.Services.Interfaces.Providers;

namespace MyPortal.Services.People;

public class SwfCensusService(
    IAuthorizationService authorizationService,
    ILogger<SwfCensusService> logger,
    ISwfCensusRepository swfCensusRepository,
    IDateTimeProvider dateTimeProvider)
    : BaseService(authorizationService, logger), ISwfCensusService
{
    private const string CollectionName = "School Workforce Census";
    private const int CollectionYear = 2026;

    private sealed record Assembly(
        SwfCensusHeaderRow? Header,
        IReadOnlyList<SwfCensusMemberRow> Members,
        ILookup<Guid, SwfCensusAbsenceRow> Absences,
        ILookup<Guid, SwfCensusAllowanceRow> Allowances,
        DateTime ReferenceDate);

    public async Task<SwfCensusPreviewResponse> GetPreviewAsync(DateTime? referenceDate,
        CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.ViewAllStaffEmploymentDetails,
            cancellationToken);

        var data = await AssembleAsync(referenceDate, cancellationToken);

        var response = new SwfCensusPreviewResponse
        {
            ReferenceDate = data.ReferenceDate,
            Year = CollectionYear,
            LaNumber = data.Header?.LaNumber,
            Estab = data.Header?.Estab,
            Urn = data.Header?.Urn,
            MemberCount = data.Members.Count
        };

        foreach (var m in data.Members)
        {
            var issues = ReadinessIssues(m).ToList();
            response.Issues.AddRange(issues);
            response.Members.Add(new SwfCensusMemberSummary
            {
                StaffMemberId = m.StaffMemberId,
                Name = $"{m.FamilyName}, {m.GivenName}",
                Trn = m.TeacherNumber,
                PostCode = m.PostCode,
                RoleCode = m.RoleCode,
                HasContract = m.ContractId.HasValue,
                AbsenceCount = data.Absences[m.StaffMemberId].Count(),
                IssueCount = issues.Count
            });
        }

        response.IssueCount = response.Issues.Count;
        return response;
    }

    public async Task<string> GenerateXmlAsync(DateTime? referenceDate, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.ViewAllStaffEmploymentDetails,
            cancellationToken);

        var data = await AssembleAsync(referenceDate, cancellationToken);
        var doc = new XDocument(new XDeclaration("1.0", "UTF-8", null), BuildMessage(data));
        return doc.Declaration + Environment.NewLine + doc;
    }

    private async Task<Assembly> AssembleAsync(DateTime? referenceDate, CancellationToken cancellationToken)
    {
        var refDate = (referenceDate ?? new DateTime(CollectionYear, 11, 5)).Date;
        var absenceFrom = new DateTime(CollectionYear - 1, 8, 1);

        var header = await swfCensusRepository.GetHeaderAsync(cancellationToken);
        var members = await swfCensusRepository.GetMembersAsync(refDate, cancellationToken);
        var absences = await swfCensusRepository.GetAbsencesAsync(absenceFrom, refDate, cancellationToken);
        var allowances = await swfCensusRepository.GetAllowancesAsync(refDate, cancellationToken);

        return new Assembly(
            header,
            members,
            absences.ToLookup(a => a.StaffMemberId),
            allowances.ToLookup(a => a.ContractId),
            refDate);
    }

    private static IEnumerable<SwfCensusReadinessIssue> ReadinessIssues(SwfCensusMemberRow m)
    {
        var name = $"{m.FamilyName}, {m.GivenName}";
        SwfCensusReadinessIssue Issue(string field, string detail) =>
            new() { StaffMemberId = m.StaffMemberId, StaffName = name, Field = field, Detail = detail };

        if (string.IsNullOrWhiteSpace(m.Sex) || (m.Sex != "M" && m.Sex != "F"))
            yield return Issue("Sex", "Missing or not M/F.");
        if (m.BirthDate is null)
            yield return Issue("Date of birth", "Missing.");
        if (string.IsNullOrWhiteSpace(m.EthnicityCode))
            yield return Issue("Ethnicity", "No DfE ethnicity code.");
        if ((m.Qts || m.Eyts) && string.IsNullOrWhiteSpace(m.TeacherNumber))
            yield return Issue("Teacher number", "Qualified teacher without a TRN.");

        if (m.ContractId is null)
        {
            yield return Issue("Contract", "No open contract on the census date.");
            yield break;
        }

        if (string.IsNullOrWhiteSpace(m.PostCode) && string.IsNullOrWhiteSpace(m.RoleCode))
            yield return Issue("Post/role", "No DfE post or role code on the contract.");
        if (m.BasePay is null)
            yield return Issue("Base pay", "Missing on the contract.");
    }

    private XElement BuildMessage(Assembly data)
    {
        var members = new XElement("SchoolWorkforceMembers",
            data.Members.Select(m => BuildMember(m, data)));

        return new XElement("Message",
            BuildHeader(data),
            new XElement("SchoolWorkforceModules",
                new XElement("ContractOrServiceIncluded", "true"),
                new XElement("AbsencesIncluded", "true"),
                new XElement("CurriculumsIncluded", "false"),
                new XElement("QualificationsIncluded", "false")),
            members);
    }

    private XElement BuildHeader(Assembly data)
    {
        var now = dateTimeProvider.UtcNow;
        return new XElement("Header",
            new XElement("CollectionDetails",
                new XElement("Collection", CollectionName),
                new XElement("Year", CollectionYear),
                new XElement("ReferenceDate", Date(data.ReferenceDate))),
            new XElement("Source",
                new XElement("SourceLevel", "S"),
                new XElement("LEA", data.Header?.LaNumber),
                new XElement("Estab", data.Header?.Estab),
                new XElement("SoftwareCode", "MYP"),
                new XElement("Release", "1"),
                new XElement("Xversion", "1.5"),
                new XElement("SerialNo", "001"),
                new XElement("DateTime", now.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture))));
    }

    private XElement BuildMember(SwfCensusMemberRow m, Assembly data)
    {
        var member = new XElement("SchoolWorkforceMember", BuildStaffDetails(m));

        if (m.ContractId.HasValue)
        {
            member.Add(new XElement("ContractOrServiceGroup", BuildContract(m, data)));
        }

        var absences = data.Absences[m.StaffMemberId].ToList();
        if (absences.Count > 0)
        {
            member.Add(new XElement("Absences", absences.Select(BuildAbsence)));
        }

        return member;
    }

    private static XElement BuildStaffDetails(SwfCensusMemberRow m)
    {
        var details = new XElement("StaffDetails");
        Add(details, "TeacherNumber", m.TeacherNumber);
        details.Add(new XElement("StaffMemberName",
            new XElement("PersonFamilyName", m.FamilyName),
            new XElement("GivenNames", new XElement("GivenName", new XElement("PersonGivenName", m.GivenName)))));
        if (!string.IsNullOrWhiteSpace(m.FormerFamilyName))
            details.Add(new XElement("FormerFamilyNames", new XElement("PersonFamilyName", m.FormerFamilyName)));
        Add(details, "NINumber", m.NiNumber);
        Add(details, "Sex", m.Sex);
        Add(details, "PersonBirthDate", Date(m.BirthDate));
        Add(details, "Ethnicity", m.EthnicityCode);
        Add(details, "Disability", m.DisabilityCode);
        details.Add(new XElement("QTS", Bool(m.Qts)));
        details.Add(new XElement("QTLS", Bool(m.Qtls)));
        details.Add(new XElement("EYTS", Bool(m.Eyts)));
        details.Add(new XElement("HLTAStatus", Bool(m.Hlta)));
        Add(details, "QTSRoute", m.QtsRouteCode);
        details.Add(new XElement("NewlyQualifiedTeacher", m.InductionCompletedDate.HasValue ? "NotNQT" : "NotNQT"));
        details.Add(new XElement("SLT", Bool(m.Slt)));
        return details;
    }

    private XElement BuildContract(SwfCensusMemberRow m, Assembly data)
    {
        var contract = new XElement("ContractOrService");
        Add(contract, "ContractType", m.ContractTypeCode);
        Add(contract, "ContractStart", Date(m.ContractStart));
        Add(contract, "ContractEnd", Date(m.ContractEnd));
        Add(contract, "SchoolArrivalDate", Date(m.ArrivalDate));
        contract.Add(new XElement("DailyRate", m.DailyRate ? "Y" : "N"));
        Add(contract, "DestinationCode", m.DestinationCode);
        Add(contract, "Origin", m.OriginCode);
        Add(contract, "LeavingReason", m.LeavingReasonCode);
        contract.Add(new XElement("LASchoolLevel", "S"));

        var levelDetails = new XElement("Payments");
        Add(levelDetails, "PayRange", MapPayRange(m.PayRangeCode));
        Add(levelDetails, "BasePay", m.BasePay?.ToString("0.00", CultureInfo.InvariantCulture));
        levelDetails.Add(new XElement("SafeguardedSalary", Bool(m.SafeguardedSalary)));

        var additional = m.ContractId.HasValue
            ? data.Allowances[m.ContractId.Value].ToList()
            : new List<SwfCensusAllowanceRow>();
        XElement? additionalPayments = additional.Count > 0
            ? new XElement("AdditionalPayments", additional.Select(BuildAdditionalPayment))
            : null;

        var hours = new XElement("Hours",
            new XElement("HoursPerWeek", ActualHours(m)?.ToString("0.##", CultureInfo.InvariantCulture)),
            new XElement("FTEHours", m.FullTimeHoursPerWeek?.ToString("0.##", CultureInfo.InvariantCulture)),
            new XElement("WeeksPerYear", m.WeeksPerYear?.ToString("0.##", CultureInfo.InvariantCulture)));

        // Hours and payments hang under Post OR Role, not both.
        if (!string.IsNullOrWhiteSpace(m.PostCode))
        {
            contract.Add(new XElement("Post", m.PostCode));
            var post = new XElement("PostLevelDetails", levelDetails);
            if (additionalPayments != null) post.Add(additionalPayments);
            post.Add(hours);
            contract.Add(post);
        }
        else
        {
            var role = new XElement("Role",
                new XElement("RoleIdentifier", m.RoleCode));
            var roleDetails = new XElement("RoleLevelDetails", levelDetails);
            if (additionalPayments != null) roleDetails.Add(additionalPayments);
            roleDetails.Add(hours);
            role.Add(roleDetails);
            contract.Add(new XElement("Roles", role));
        }

        return contract;
    }

    private static XElement BuildAdditionalPayment(SwfCensusAllowanceRow a) =>
        new("AdditionalPayment",
            new XElement("PaymentType", a.PaymentTypeCode),
            new XElement("PaymentAmount", a.Amount.ToString("0.00", CultureInfo.InvariantCulture)),
            a.PayStartDate.HasValue ? new XElement("PayStartDate", Date(a.PayStartDate)) : null,
            a.PayEndDate.HasValue ? new XElement("PayEndDate", Date(a.PayEndDate)) : null);

    private static XElement BuildAbsence(SwfCensusAbsenceRow a) =>
        new("Absence",
            new XElement("FirstDayOfAbsence", Date(a.FirstDay)),
            a.LastDay.HasValue ? new XElement("LastDayOfAbsence", Date(a.LastDay)) : null,
            a.WorkingDaysLost.HasValue
                ? new XElement("WorkingDaysLost", a.WorkingDaysLost.Value.ToString("0.##", CultureInfo.InvariantCulture))
                : null,
            new XElement("AbsenceCategory", a.CategoryCode));

    private static decimal? ActualHours(SwfCensusMemberRow m) =>
        m.FullTimeHoursPerWeek.HasValue && m.Fte.HasValue ? m.FullTimeHoursPerWeek * m.Fte : m.FullTimeHoursPerWeek;

    private static void Add(XElement parent, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value)) parent.Add(new XElement(name, value));
    }

    // TODO: verify MS/UP against the CBDS pay-range code list (N00568).
    private static string? MapPayRange(string? payScaleCode) => payScaleCode?.ToUpperInvariant() switch
    {
        "MPS" => "MS", // Main pay range
        "UPS" => "UP", // Upper pay range
        "LDR" => "LD", // Leadership
        "UNQ" => "UT", // Unqualified teachers
        _ => null
    };

    private static string Bool(bool value) => value ? "true" : "false";

    private static string? Date(DateTime? value) => value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
