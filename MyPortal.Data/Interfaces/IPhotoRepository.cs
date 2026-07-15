using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

/// <summary>
/// A person's photo row (<c>Person.PhotoId → Photos.Id</c>), wrapping the document that holds the
/// image bytes. Plain CRUD — a photo is always looked up by id (the person carries that id
/// directly), so no custom queries are needed.
/// </summary>
public interface IPhotoRepository : IEntityRepository<Photo>
{
}
