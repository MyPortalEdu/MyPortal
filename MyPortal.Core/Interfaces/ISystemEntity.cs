namespace MyPortal.Core.Interfaces
{
    /// <summary>
    /// A row managed by MyPortal and protected from direct user mutation: EntityRepository's
    /// UpdateAsync/DeleteAsync throw SystemEntityException when IsSystem is true. The guard fires
    /// ONLY for writes that go through EntityRepository — an entity written via another path (e.g.
    /// ASP.NET Identity's user/role stores) gets no protection from this interface and must enforce
    /// it itself. Parent-lifecycle cascades (the usp_*_delete_by_parent_id procs) run raw SQL and
    /// do not consult IsSystem by design.
    /// </summary>
    public interface ISystemEntity : IEntity
    {
        public bool IsSystem { get; set; }
    }
}