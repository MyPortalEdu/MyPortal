using System.Reflection;
using MyPortal.Common.Constants;
using MyPortal.Migrations.Services;

namespace MyPortal.Tests.MigrationTests
{
    /// <summary>
    /// Pins <see cref="SystemPhotos"/> to the migrations that seed those rows. Nothing in the type
    /// system ties the two together, so drift only shows up as an FK violation at photo-upload time.
    /// Reads the migrations as embedded resources — no database needed.
    /// </summary>
    [TestFixture]
    public class SystemPhotosSeedTests
    {
        private const string PhotoSupportMigration =
            "MyPortal.Migrations.Sql.Updates.20260707000000_person_photo_support.sql";

        private const string DirectoryIsSystemMigration =
            "MyPortal.Migrations.Sql.Updates.20260715000000_directory_is_system.sql";

        private static readonly Assembly MigrationsAssembly = typeof(DbUpdateService).Assembly;

        private static string ReadMigration(string resourceName)
        {
            using var stream = MigrationsAssembly.GetManifestResourceStream(resourceName);

            // Also guards the csproj footgun: migrations are listed individually, so an unlisted
            // .sql file is invisible to the runner and silently never applies.
            Assert.That(stream, Is.Not.Null,
                $"Migration '{resourceName}' is not an embedded resource. Add an explicit " +
                "<EmbeddedResource Include=\"...\" /> entry for it in MyPortal.Migrations.csproj.");

            using var reader = new StreamReader(stream!);
            return reader.ReadToEnd();
        }

        [Test]
        public void DirectoryId_IsSeededBy_PhotoSupportMigration()
        {
            var sql = ReadMigration(PhotoSupportMigration);

            Assert.That(sql, Does.Contain(SystemPhotos.DirectoryId.ToString("D")).IgnoreCase,
                $"SystemPhotos.DirectoryId ({SystemPhotos.DirectoryId}) does not appear in " +
                $"{PhotoSupportMigration}. Photo documents are written into this directory, so a " +
                "mismatch fails the Documents.DirectoryId FK.");
        }

        [Test]
        public void PhotographDocumentTypeId_IsSeededBy_PhotoSupportMigration()
        {
            var sql = ReadMigration(PhotoSupportMigration);

            Assert.That(sql, Does.Contain(SystemPhotos.PhotographDocumentTypeId.ToString("D")).IgnoreCase,
                $"SystemPhotos.PhotographDocumentTypeId ({SystemPhotos.PhotographDocumentTypeId}) does " +
                $"not appear in {PhotoSupportMigration}. A mismatch fails the Documents.TypeId FK.");
        }

        [Test]
        public void DirectoryId_IsFlaggedSystemBy_DirectoryIsSystemMigration()
        {
            var sql = ReadMigration(DirectoryIsSystemMigration);

            Assert.Multiple(() =>
            {
                Assert.That(sql, Does.Contain(SystemPhotos.DirectoryId.ToString("D")).IgnoreCase,
                    "The directory flagged IsSystem is not SystemPhotos.DirectoryId, leaving the photos " +
                    "directory user-deletable.");

                Assert.That(sql, Does.Contain("IsSystem").IgnoreCase,
                    "The migration no longer references IsSystem, which EntityRepository reads to " +
                    "refuse updates/deletes.");
            });
        }
    }
}
