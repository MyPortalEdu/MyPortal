using MyPortal.FileStorage.Interfaces;

namespace MyPortal.FileStorage.Helpers
{
    public class DefaultStorageKeyGenerator : IStorageKeyGenerator
    {
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
            var shard = id.Substring(0, 2);

            return $"{year}/{month}/{shard}/{id}{ext}";
        }
    }
}
