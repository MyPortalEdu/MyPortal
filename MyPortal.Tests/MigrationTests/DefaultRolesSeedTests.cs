using System.Reflection;
using MyPortal.Common.Constants;
using MyPortal.Migrations.Services;

namespace MyPortal.Tests.MigrationTests
{
    /// <summary>
    /// Pins the well-known role ids in <see cref="SystemRoles"/> to the seed migration. UserService
    /// auto-assigns Student/Parent roles by id, so a drift between the constant and the seed only
    /// surfaces as a "Role not found" at user creation. Reads the migration as an embedded resource —
    /// no database needed.
    /// </summary>
    [TestFixture]
    public class DefaultRolesSeedTests
    {
        private const string SeedMigration =
            "MyPortal.Migrations.Sql.Updates.20260717000100_seed_default_roles.sql";

        private static string ReadSeed()
        {
            var assembly = typeof(DbUpdateService).Assembly;
            using var stream = assembly.GetManifestResourceStream(SeedMigration);

            // Also guards the csproj footgun: migrations are listed individually, so a .sql not added
            // as <EmbeddedResource> is invisible to the runner and silently never applies.
            Assert.That(stream, Is.Not.Null,
                $"Migration '{SeedMigration}' is not an embedded resource. Add an explicit " +
                "<EmbeddedResource Include=\"...\" /> entry for it in MyPortal.Migrations.csproj.");

            using var reader = new StreamReader(stream!);
            return reader.ReadToEnd();
        }

        [Test]
        public void StudentRoleId_IsSeeded()
        {
            Assert.That(ReadSeed(), Does.Contain(SystemRoles.StudentRoleId.ToString("D")).IgnoreCase,
                $"SystemRoles.StudentRoleId ({SystemRoles.StudentRoleId}) does not appear in the seed migration. " +
                "UserService auto-assigns the Student role by this id; a mismatch fails user creation.");
        }

        [Test]
        public void ParentRoleId_IsSeeded()
        {
            Assert.That(ReadSeed(), Does.Contain(SystemRoles.ParentRoleId.ToString("D")).IgnoreCase,
                $"SystemRoles.ParentRoleId ({SystemRoles.ParentRoleId}) does not appear in the seed migration. " +
                "UserService auto-assigns the Parent role by this id; a mismatch fails user creation.");
        }
    }
}
