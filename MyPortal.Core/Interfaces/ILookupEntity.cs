namespace MyPortal.Core.Interfaces
{
    public interface ILookupEntity : IEntity
    {
        public string Description { get; set; }
        public bool Active { get; set; }
    }
}