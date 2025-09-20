using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Agencies")]
    public class Agency : Entity, IDirectoryEntity, ISoftDeleteEntity
    {
        public Guid AgencyTypeId { get; set; }

        public Guid DirectoryId { get; set; }

        
        [Required, StringLength(256)]
        public required string Name { get; set; }

        [Url, StringLength(100)]
        public string? Website { get; set; }

        public bool IsDeleted { get; set; }

        public AgencyType? AgencyType { get; set; }
        public Directory? Directory { get; set; }
    }
}