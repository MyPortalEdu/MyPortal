using MyPortal.Contracts.Models.Documents;

namespace MyPortal.Services.Extensions;

public static class DirectoryTreeResponseExtensions
{
    public static bool ContainsPrivateEntities(this DirectoryTreeResponse currentTree)
    {
        return currentTree.Directory.IsPrivate || 
               (currentTree.Documents?.Any(d => d.IsPrivate) ?? false) ||
               (currentTree.Directories?.Any(e => e.ContainsPrivateEntities()) ?? false);
    }
}