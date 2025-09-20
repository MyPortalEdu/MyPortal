using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ExamSeatAllocations")]
    public class ExamSeatAllocation : Entity
    {
        public Guid SittingId { get; set; }

        public int SeatRow { get; set; }

        public int SeatColumn { get; set; }

        public Guid CandidateId { get; set; }

        public bool IsActive { get; set; }

        public bool HasAttended { get; set; }

        public ExamComponentSitting? Sitting { get; set; }
        public ExamCandidate? Candidate { get; set; }
    }
}