using MyPortal.Contracts.Enums;

namespace MyPortal.Contracts.Models.People
{
    public class PersonSummaryDto
    {
        public Guid Id { get; set; }

        public PersonType PersonType { get; set; }

        public string? Title { get; set; }

        public string? PreferredFirstName { get; set; }

        public string? PreferredLastName { get; set; }

        public required string FirstName { get; set; }

        public string? MiddleName { get; set; }

        public required string LastName { get; set; }

        public Guid? PhotoId { get; set; }

        public required string Gender { get; set; }

        public DateTime? Dob { get; set; }

        public DateTime? Deceased { get; set; }

        public Guid? EthnicityId { get; set; }
    }
}
