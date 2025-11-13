namespace MyPortal.FileStorage.Models;

public class HashedStream
{
    public string Hash { get; set; }
    public Stream UsableStream { get; set; }
}