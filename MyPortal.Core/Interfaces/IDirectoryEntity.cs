using Directory = MyPortal.Core.Entities.Directory;

namespace MyPortal.Core.Interfaces
{
    public interface IDirectoryEntity : IEntity
    {
        public Guid DirectoryId { get; set; }
        public Directory? Directory { get; set; }
    }
}