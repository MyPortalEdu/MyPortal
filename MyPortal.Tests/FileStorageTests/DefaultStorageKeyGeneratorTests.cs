using MyPortal.FileStorage.Helpers;

namespace MyPortal.Tests.FileStorageTests
{
    [TestFixture]
    public class DefaultStorageKeyGeneratorTests
    {
        [Test]
        public void GenerateKey_ShouldReturnExpectedKeyFormat()
        {
            // Arrange
            var generator = new DefaultStorageKeyGenerator();
            var fileName = "document.pdf";

            // Act
            var result = generator.Generate(fileName);

            // Assert
            var now = DateTime.UtcNow;

            Assert.That(result, Does.StartWith($"{now.Year:0000}/{now.Month:00}"));
            Assert.That(result, Does.EndWith(".pdf"));
        }
    }
}
