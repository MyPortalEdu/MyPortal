using Microsoft.Extensions.Options;
using MyPortal.Common.Options;
using MyPortal.FileStorage.Providers;

namespace MyPortal.Tests.FileStorageTests
{
    [TestFixture]
    public class FileSystemStorageProviderTests
    {
        private string _tempDirectory;
        private FileSystemStorageProvider _provider;

        [SetUp]
        public void SetUp()
        {
            // Create a temporary directory for testing
            _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDirectory);

            var options = Options.Create(new FileStorageOptions
            {
                FileSystem = new FileSystemOptions
                {
                    RootPath = _tempDirectory
                }
            });

            _provider = new FileSystemStorageProvider(options);
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up the temporary directory
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }

        [Test]
        public async Task SaveFileAsync_WithValidKey_ShouldSaveFile()
        {
            // Arrange
            var storageKey = "test/document.txt";
            var content = "Test content";
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

            // Act
            await _provider.SaveFileAsync(storageKey, stream, "text/plain", CancellationToken.None);

            // Assert
            var filePath = Path.Combine(_tempDirectory, "test", "document.txt");
            Assert.That(File.Exists(filePath), Is.True);
            var savedContent = await File.ReadAllTextAsync(filePath);
            Assert.That(savedContent, Is.EqualTo(content));
        }

        [Test]
        public void SaveFileAsync_WithPathTraversalAttempt_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var storageKey = "../../etc/passwd";
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("malicious content"));

            // Act & Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _provider.SaveFileAsync(storageKey, stream, "text/plain", CancellationToken.None));
        }

        [Test]
        public void OpenReadFileAsync_WithPathTraversalAttempt_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var storageKey = "../../../sensitive-file.txt";

            // Act & Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _provider.OpenReadFileAsync(storageKey, CancellationToken.None));
        }

        [Test]
        public void DeleteFileAsync_WithPathTraversalAttempt_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var storageKey = "../../important-file.txt";

            // Act & Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _provider.DeleteFileAsync(storageKey, CancellationToken.None));
        }

        [Test]
        public void ExistsAsync_WithPathTraversalAttempt_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var storageKey = "../../../etc/hosts";

            // Act & Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _provider.ExistsAsync(storageKey, CancellationToken.None));
        }

        [Test]
        public async Task OpenReadFileAsync_WithValidKey_ShouldReturnStream()
        {
            // Arrange
            var storageKey = "documents/test.txt";
            var content = "Test file content";
            var filePath = Path.Combine(_tempDirectory, "documents");
            Directory.CreateDirectory(filePath);
            await File.WriteAllTextAsync(Path.Combine(filePath, "test.txt"), content);

            // Act
            using var stream = await _provider.OpenReadFileAsync(storageKey, CancellationToken.None);
            using var reader = new StreamReader(stream);
            var result = await reader.ReadToEndAsync();

            // Assert
            Assert.That(result, Is.EqualTo(content));
        }

        [Test]
        public async Task DeleteFileAsync_WithValidKey_ShouldDeleteFile()
        {
            // Arrange
            var storageKey = "temp/deleteme.txt";
            var filePath = Path.Combine(_tempDirectory, "temp");
            Directory.CreateDirectory(filePath);
            var fullFilePath = Path.Combine(filePath, "deleteme.txt");
            await File.WriteAllTextAsync(fullFilePath, "content");

            // Act
            await _provider.DeleteFileAsync(storageKey, CancellationToken.None);

            // Assert
            Assert.That(File.Exists(fullFilePath), Is.False);
        }

        [Test]
        public async Task ExistsAsync_WithExistingFile_ShouldReturnTrue()
        {
            // Arrange
            var storageKey = "files/exists.txt";
            var filePath = Path.Combine(_tempDirectory, "files");
            Directory.CreateDirectory(filePath);
            await File.WriteAllTextAsync(Path.Combine(filePath, "exists.txt"), "content");

            // Act
            var exists = await _provider.ExistsAsync(storageKey, CancellationToken.None);

            // Assert
            Assert.That(exists, Is.True);
        }

        [Test]
        public async Task ExistsAsync_WithNonExistingFile_ShouldReturnFalse()
        {
            // Arrange
            var storageKey = "files/doesnotexist.txt";

            // Act
            var exists = await _provider.ExistsAsync(storageKey, CancellationToken.None);

            // Assert
            Assert.That(exists, Is.False);
        }
    }
}
