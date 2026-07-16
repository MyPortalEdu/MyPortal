using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Students")]
    public class Student : Entity, IAuditableEntity, ISoftDeleteEntity, IVersionedEntity
    {
        public Guid PersonId { get; set; }

        public int AdmissionNumber { get; set; }

        public DateTime? DateStarting { get; set; }

        public DateTime? DateLeaving { get; set; }

        public bool FreeSchoolMeals { get; set; }

        public Guid? SenStatusId { get; set; }

        public Guid? SenTypeId { get; set; }

        public Guid? EnrolmentStatusId { get; set; }

        public Guid? BoarderStatusId { get; set; }

        public bool PupilPremium { get; set; }

        // Which pupil premium the pupil attracts (CBDS CS106) + FSM category (CS133).
        public Guid? PupilPremiumIndicatorId { get; set; }

        public Guid? FsmCategoryId { get; set; }

        // Date the pupil's current SEN need / register entry began.
        public DateTime? SenStartDate { get; set; }

        // Proficiency in English / EAL stage (CBDS CS089) + the date assessed — pupil-only.
        public Guid? EnglishProficiencyId { get; set; }

        public DateTime? EnglishProficiencyDate { get; set; }

        [StringLength(13)]
        public string? Upn { get; set; }

        // Former UPN where the pupil's UPN has changed (CTF / census).
        [StringLength(13)]
        public string? FormerUpn { get; set; }

        // Reason a UPN is absent (CBDS CS051) — census rejects a blank UPN without one.
        public Guid? UpnUnknownReasonId { get; set; }

        // Unique Learner Number — required for 16-19 / post-16 returns.
        [StringLength(10)]
        public string? Uln { get; set; }

        // LA cross-reference identifier (CTF / B2B).
        [StringLength(20)]
        public string? LaChildId { get; set; }

        // Looked-after-child flag + the caring local authority (reuses LocalAuthorities).
        public bool InCare { get; set; }

        public Guid? CaringAuthorityId { get; set; }

        // Post-looked-after arrangement, e.g. adopted / SGO / CAO (CBDS CS087) — pupil-premium-plus.
        public Guid? PostLookedAfterArrangementId { get; set; }

        // High-needs / top-up funding flag (school census).
        public bool TopUpFunding { get; set; }

        // FSM eligibility window + review date (Ever-6 / pupil-premium calculations).
        public DateTime? FsmEligibilityStartDate { get; set; }

        public DateTime? FsmEligibilityEndDate { get; set; }

        public DateTime? FsmReviewDate { get; set; }

        // Part-time pupil (school census — affects funding / attendance expectations).
        public bool IsPartTime { get; set; }

        // Census provision-placement flags.
        public bool SenUnitMember { get; set; }

        public bool ResourcedProvisionMember { get; set; }

        // Service child (armed-forces family) indicator (CBDS CS006) — service premium.
        public Guid? ServiceChildIndicatorId { get; set; }

        // Young-carer indicator (CBDS CS118) — school census 2023+.
        public Guid? YoungCarerIndicatorId { get; set; }

        // Kinship-care indicator (CBDS CS134).
        public Guid? KinshipCareIndicatorId { get; set; }

        public bool IsDeleted { get; set; }

        public Person? Person { get; set; }
        public SenStatus? SenStatus { get; set; }
        public SenType? SenType { get; set; }
        public EnrolmentStatus? EnrolmentStatus { get; set; }
        public BoarderStatus? BoarderStatus { get; set; }
        public UpnUnknownReason? UpnUnknownReason { get; set; }
        public PupilPremiumIndicator? PupilPremiumIndicator { get; set; }
        public FsmCategory? FsmCategory { get; set; }
        public EnglishProficiency? EnglishProficiency { get; set; }
        public LocalAuthority? CaringAuthority { get; set; }
        public PostLookedAfterArrangement? PostLookedAfterArrangement { get; set; }
        public ServiceChildIndicator? ServiceChildIndicator { get; set; }
        public YoungCarerIndicator? YoungCarerIndicator { get; set; }
        public KinshipCareIndicator? KinshipCareIndicator { get; set; }
        
        public Guid CreatedById { get; set; }
        public string CreatedByIpAddress { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid LastModifiedById { get; set; }
        public string LastModifiedByIpAddress { get; set; } = string.Empty;
        public DateTime LastModifiedAt { get; set; }
        public User? CreatedBy { get; set; }
        public User? LastModifiedBy { get; set; }
        public long Version { get; set; }
    }
}