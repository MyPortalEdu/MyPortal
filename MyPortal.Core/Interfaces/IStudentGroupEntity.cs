using MyPortal.Core.Entities;

namespace MyPortal.Core.Interfaces
{
    public interface IStudentGroupEntity : IEntity
    {
        Guid StudentGroupId { get; set; }
        StudentGroup? StudentGroup { get; set; }
    }
}