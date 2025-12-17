using MyPortal.FileStorage.Interfaces;

namespace MyPortal.FileStorage.Helpers
{
    /// <summary>
    /// Provides a default implementation for generating unique, date-based storage keys for files.
    /// </summary>
    /// <remarks>This class is intended for use in scenarios where files need to be organized in a
    /// hierarchical or sharded directory structure, such as cloud or distributed storage systems. The generated keys
    /// help distribute files evenly and minimize naming collisions by incorporating the current UTC year and month, a
    /// shard derived from a GUID, and the original file extension.</remarks>
    public class DefaultStorageKeyGenerator : IStorageKeyGenerator
    {
        /// <summary>
        /// Generates a unique, date-based file path for storing a file with the specified original file name.
        /// </summary>
        /// <remarks>The generated path uses the current UTC year and month, a two-character shard derived
        /// from a GUID, and a unique file name to help distribute files evenly and avoid naming collisions. This method
        /// is suitable for organizing files in storage systems that benefit from hierarchical or sharded directory
        /// structures.</remarks>
        /// <param name="originalFileName">The original file name, including its extension. The extension is used to preserve the file type in the
        /// generated path.</param>
        /// <returns>A string representing the generated file path, organized by year, month, and a shard based on a unique
        /// identifier. The returned path includes the original file extension.</returns>
        /// <exception cref="ArgumentException">Thrown if originalFileName does not contain a valid file extension.</exception>
        public string Generate(string originalFileName)
        {
            var utcNow = DateTime.UtcNow;

            var ext = Path.GetExtension(originalFileName);

            if (string.IsNullOrWhiteSpace(ext))
            {
                throw new ArgumentException("Invalid file name.", nameof(originalFileName));
            }

            var id = Guid.NewGuid().ToString("N");
            var year = utcNow.Year.ToString("0000");
            var month = utcNow.Month.ToString("00");
            var shard = id[..2];

            return $"{year}/{month}/{shard}/{id}{ext}";
        }
    }
}
