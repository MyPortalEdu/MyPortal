namespace MyPortal.Core.Interfaces
{
    /// <summary>
    /// Defines an entity that is able to be created by MyPortal.
    /// </summary>
    public interface ISystemEntity : IEntity
    {
        /// <summary>
        /// Marks the row as managed by MyPortal and protected from direct user mutation.
        /// EntityRepository.UpdateAsync and DeleteAsync both throw SystemEntityException
        /// when this is true, so the row can't be edited or removed via the standard
        /// repository methods.
        ///
        /// Cascade deletes from a parent (e.g. wiping an academic year's holidays and
        /// their underlying DiaryEvents) intentionally bypass this guard by going
        /// through usp_*_delete_by_&lt;parent&gt;_id stored procedures that operate in raw
        /// SQL — IsSystem is not consulted by those procs. If you add a system-flagged
        /// child entity, the matching DeleteBy&lt;parent&gt;Async path needs its own SP that
        /// knows to clean up the child rows.
        /// </summary>
        public bool IsSystem { get; set; }
    }
}