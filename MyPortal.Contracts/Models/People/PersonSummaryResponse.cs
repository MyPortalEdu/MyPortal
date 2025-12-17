using MyPortal.Contracts.Enums;

namespace MyPortal.Contracts.Models.People
{
    public class PersonSummaryResponse
    {
        public Guid Id { get; set; }

        public PersonType PersonType { get; set; }

        public string? Title { get; set; }

        public string? PreferredFirstName { get; set; }

        public string? PreferredLastName { get; set; }

        public string FirstName { get; set; } = null!;

        public string? MiddleName { get; set; }

        public string LastName { get; set; } = null!;

        public Guid? PhotoId { get; set; }

        public string Gender { get; set; } = null!;

        public DateTime? Dob { get; set; }

        public DateTime? Deceased { get; set; }

        public Guid? EthnicityId { get; set; }
    }
}
