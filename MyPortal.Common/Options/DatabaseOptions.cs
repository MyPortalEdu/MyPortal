using System.ComponentModel.DataAnnotations;

namespace MyPortal.Common.Options
{
    public sealed class DatabaseOptions
    {
        [Required]
        public string Provider { get; init; } = "SqlServer";

        [Required, MinLength(3)]
        public string ConnectionString { get; init; } = default!;
    }
}
