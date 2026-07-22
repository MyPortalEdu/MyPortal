namespace MyPortal.Contracts.Models.Bulletins;

public class BulletinPinRequest
{
    public bool IsPinned { get; set; }
    
    public long ExpectedVersion { get; set; }
}
