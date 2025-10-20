namespace MyPortal.Contracts.Enums
{
    [Flags]
    public enum PersonType
    {
        None = 0,
        Student = 1 << 0,
        Staff = 1 << 1,
        Contact = 1 << 2,
        Agent = 1 << 3
    }
}
