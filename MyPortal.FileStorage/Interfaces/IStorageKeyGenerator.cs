namespace MyPortal.FileStorage.Interfaces
{
    public interface IStorageKeyGenerator
    {
        /// <summary>
        /// Generates a storage key (path-like string) for an uploaded file.
        /// Should be safe across all providers (FileSystem, Azure Blob).
        /// </summary>
        string Generate(string originalFileName);
    }
}
