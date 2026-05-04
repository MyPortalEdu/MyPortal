using System.Data;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Enums;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Options;
using MyPortal.Contracts.Models.Documents;
using MyPortal.Core.Entities;
using MyPortal.FileStorage.Interfaces;
using MyPortal.Services.Documents;
using MyPortal.Services.Interfaces;
using Task = System.Threading.Tasks.Task;
using MyPortal.Data.Interfaces;

namespace MyPortal.Tests.ServiceTests;

[TestFixture]
public class DocumentServiceTests
{
    private static readonly Guid CurrentUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OtherUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private Mock<IAuthorizationService> _authorizationService;
    private Mock<ILogger<DocumentService>> _logger;
    private Mock<IDocumentRepository> _documentRepository;
    private Mock<IDocumentTypeRepository> _documentTypeRepository;
    private Mock<IStorageKeyGenerator> _storageKeyGenerator;
    private Mock<IFileStorageProvider> _storageProvider;
    private Mock<IValidationService> _validationService;
    private Mock<IOptions<FileStorageOptions>> _fileStorageOptions;

    private DocumentService _service;

    [SetUp]
    public void Setup()
    {
        _authorizationService = new Mock<IAuthorizationService>(MockBehavior.Strict);
        _logger = new Mock<ILogger<DocumentService>>(MockBehavior.Loose);
        _documentRepository = new Mock<IDocumentRepository>(MockBehavior.Strict);
        _documentTypeRepository = new Mock<IDocumentTypeRepository>(MockBehavior.Strict);
        _storageKeyGenerator = new Mock<IStorageKeyGenerator>(MockBehavior.Strict);
        _storageProvider = new Mock<IFileStorageProvider>(MockBehavior.Strict);
        _validationService = new Mock<IValidationService>(MockBehavior.Strict);
        _fileStorageOptions = new Mock<IOptions<FileStorageOptions>>(MockBehavior.Strict);

        _fileStorageOptions.Setup(o => o.Value).Returns(new FileStorageOptions { MaxFileSizeBytes = 50 * 1024 * 1024 });
        _validationService.Setup(v => v.ValidateAsync(It.IsAny<DocumentUpsertRequest>())).Returns(Task.CompletedTask);
        _authorizationService.Setup(a => a.GetCurrentUserId()).Returns(CurrentUserId);
        _authorizationService.Setup(a => a.GetCurrentUserType()).Returns(UserType.Staff);

        _service = new DocumentService(
            _authorizationService.Object,
            _logger.Object,
            _documentRepository.Object,
            _documentTypeRepository.Object,
            _storageKeyGenerator.Object,
            _storageProvider.Object,
            _validationService.Object,
            _fileStorageOptions.Object
        );
    }

    private static MemoryStream Stream(string content) => new(Encoding.UTF8.GetBytes(content));

    private static DocumentUpsertRequest MakeUpsert(Stream? content, long? sizeBytes, bool isPrivate = false) =>
        new()
        {
            TypeId = Guid.NewGuid(),
            DirectoryId = Guid.NewGuid(),
            Title = "doc",
            Description = null,
            IsPrivate = isPrivate,
            FileName = "doc.txt",
            ContentType = "text/plain",
            Content = content,
            SizeBytes = sizeBytes
        };

    // ─── CreateDocumentAsync ─────────────────────────────────────────────────

    [Test]
    public async Task CreateDocumentAsync_HappyPath_HashesAndSavesAndInserts()
    {
        const string body = "hello world";
        var bytes = Encoding.UTF8.GetBytes(body);
        var model = MakeUpsert(Stream(body), bytes.Length);

        _storageKeyGenerator.Setup(g => g.Generate("doc.txt")).Returns("2026/04/abcdef.txt");
        _storageProvider.Setup(s => s.SaveFileAsync("2026/04/abcdef.txt", It.IsAny<Stream>(), "text/plain",
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _documentRepository.Setup(r => r.InsertAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync((Document d, CancellationToken _, IDbTransaction? _) => d);
        var detailsDto = new DocumentDetailsResponse { Title = "doc" };
        _documentRepository.Setup(r => r.GetDetailsByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(detailsDto);

        var result = await _service.CreateDocumentAsync(model, CancellationToken.None);

        Assert.That(result, Is.SameAs(detailsDto));
        // Persisted SizeBytes is the actual stream length, not whatever the client claimed.
        _documentRepository.Verify(r => r.InsertAsync(
            It.Is<Document>(d => d.SizeBytes == bytes.Length && d.StorageKey == "2026/04/abcdef.txt"),
            It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Once);
    }

    [Test]
    public void CreateDocumentAsync_Throws_WhenContentNull()
    {
        var model = MakeUpsert(content: null, sizeBytes: 10);
        Assert.That(async () => await _service.CreateDocumentAsync(model, CancellationToken.None),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void CreateDocumentAsync_Throws_WhenSizeBytesZero()
    {
        var model = MakeUpsert(Stream("x"), 0);
        Assert.That(async () => await _service.CreateDocumentAsync(model, CancellationToken.None),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void CreateDocumentAsync_Throws_Forbidden_WhenNonStaffUploadsPrivate()
    {
        _authorizationService.Setup(a => a.GetCurrentUserType()).Returns(UserType.Student);

        var model = MakeUpsert(Stream("x"), 1, isPrivate: true);

        Assert.That(async () => await _service.CreateDocumentAsync(model, CancellationToken.None),
            Throws.TypeOf<ForbiddenException>());

        _storageProvider.Verify(s => s.SaveFileAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void CreateDocumentAsync_Throws_WhenDeclaredSizeDoesNotMatchActual()
    {
        const string body = "real content";
        var actualLength = Encoding.UTF8.GetByteCount(body);
        // Client claims a smaller size than the actual stream.
        var model = MakeUpsert(Stream(body), actualLength - 5);

        _storageKeyGenerator.Setup(g => g.Generate(It.IsAny<string>())).Returns("k");

        Assert.That(async () => await _service.CreateDocumentAsync(model, CancellationToken.None),
            Throws.TypeOf<ArgumentException>().With.Property("ParamName").EqualTo("SizeBytes"));

        // Critical: storage save runs (it's needed to know the actual length on a non-seekable
        // stream — but the seekable MemoryStream path here means we reject before save). Either
        // way, no DB row should be inserted.
        _documentRepository.Verify(r => r.InsertAsync(It.IsAny<Document>(),
            It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Never);
    }

    [Test]
    public void CreateDocumentAsync_Throws_WhenContentExceedsMaxFileSize()
    {
        // Tighten the cap so we can hit it without allocating 50 MB.
        _fileStorageOptions.Setup(o => o.Value).Returns(new FileStorageOptions { MaxFileSizeBytes = 8 });
        _service = new DocumentService(
            _authorizationService.Object, _logger.Object, _documentRepository.Object,
            _documentTypeRepository.Object, _storageKeyGenerator.Object, _storageProvider.Object,
            _validationService.Object, _fileStorageOptions.Object);

        var bytes = new byte[64];
        var model = MakeUpsert(new MemoryStream(bytes), bytes.Length);

        _storageKeyGenerator.Setup(g => g.Generate(It.IsAny<string>())).Returns("k");

        Assert.That(async () => await _service.CreateDocumentAsync(model, CancellationToken.None),
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("exceeds maximum size"));
    }

    // ─── UpdateDocumentAsync ─────────────────────────────────────────────────

    [Test]
    public async Task UpdateDocumentAsync_NoNewContent_UpdatesMetadataOnly()
    {
        var docId = Guid.NewGuid();
        var existing = new Document
        {
            Id = docId, Title = "old", DirectoryId = Guid.NewGuid(),
            StorageKey = "old/key", FileName = "old.txt", ContentType = "text/plain",
            CreatedById = CurrentUserId
        };

        _documentRepository.Setup(r => r.GetByIdAsync(docId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>())).ReturnsAsync(existing);
        _documentRepository.Setup(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>())).ReturnsAsync(existing);
        _documentRepository.Setup(r => r.GetDetailsByIdAsync(docId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DocumentDetailsResponse { Title = "new" });

        var model = MakeUpsert(content: null, sizeBytes: null);
        model.Title = "new";

        await _service.UpdateDocumentAsync(docId, model, CancellationToken.None);

        Assert.That(existing.Title, Is.EqualTo("new"));
        Assert.That(existing.StorageKey, Is.EqualTo("old/key"));
        _storageProvider.Verify(s => s.SaveFileAsync(It.IsAny<string>(), It.IsAny<Stream>(),
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _storageProvider.Verify(s => s.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task UpdateDocumentAsync_NewContent_SavesNew_UpdatesDb_DeletesOldBlob()
    {
        var docId = Guid.NewGuid();
        const string body = "new body";
        var existing = new Document
        {
            Id = docId, Title = "old", DirectoryId = Guid.NewGuid(),
            StorageKey = "old/key", FileName = "old.txt", ContentType = "text/plain",
            CreatedById = CurrentUserId
        };

        _documentRepository.Setup(r => r.GetByIdAsync(docId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>())).ReturnsAsync(existing);
        _storageKeyGenerator.Setup(g => g.Generate("doc.txt")).Returns("new/key");
        _storageProvider.Setup(s => s.SaveFileAsync("new/key", It.IsAny<Stream>(), "text/plain",
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _documentRepository.Setup(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>())).ReturnsAsync(existing);
        _storageProvider.Setup(s => s.DeleteFileAsync("old/key", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _documentRepository.Setup(r => r.GetDetailsByIdAsync(docId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DocumentDetailsResponse { Title = "new" });

        var bytes = Encoding.UTF8.GetByteCount(body);
        var model = MakeUpsert(Stream(body), bytes);
        model.Title = "new";

        await _service.UpdateDocumentAsync(docId, model, CancellationToken.None);

        Assert.That(existing.StorageKey, Is.EqualTo("new/key"));
        Assert.That(existing.SizeBytes, Is.EqualTo(bytes));
        _storageProvider.Verify(s => s.SaveFileAsync("new/key", It.IsAny<Stream>(), "text/plain",
            It.IsAny<CancellationToken>()), Times.Once);
        _storageProvider.Verify(s => s.DeleteFileAsync("old/key", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task UpdateDocumentAsync_DbUpdateFailure_CleansUpNewBlob()
    {
        var docId = Guid.NewGuid();
        var existing = new Document
        {
            Id = docId, Title = "old", DirectoryId = Guid.NewGuid(),
            StorageKey = "old/key", FileName = "old.txt", ContentType = "text/plain",
            CreatedById = CurrentUserId
        };

        _documentRepository.Setup(r => r.GetByIdAsync(docId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>())).ReturnsAsync(existing);
        _storageKeyGenerator.Setup(g => g.Generate(It.IsAny<string>())).Returns("new/key");
        _storageProvider.Setup(s => s.SaveFileAsync("new/key", It.IsAny<Stream>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _documentRepository.Setup(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ThrowsAsync(new InvalidOperationException("db down"));
        _storageProvider.Setup(s => s.DeleteFileAsync("new/key", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        const string body = "x";
        var model = MakeUpsert(Stream(body), Encoding.UTF8.GetByteCount(body));

        Assert.That(async () => await _service.UpdateDocumentAsync(docId, model, CancellationToken.None),
            Throws.TypeOf<InvalidOperationException>());

        // Old blob is preserved (DB update failed, so the row still points at it).
        _storageProvider.Verify(s => s.DeleteFileAsync("old/key", It.IsAny<CancellationToken>()),
            Times.Never);
        // New blob is cleaned up so we don't leave an orphan referenced by no row.
        _storageProvider.Verify(s => s.DeleteFileAsync("new/key", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void UpdateDocumentAsync_Throws_Forbidden_WhenNonStaffNonOwner()
    {
        _authorizationService.Setup(a => a.GetCurrentUserType()).Returns(UserType.Student);
        var docId = Guid.NewGuid();
        var existing = new Document
        {
            Id = docId, Title = "x", DirectoryId = Guid.NewGuid(),
            StorageKey = "k", FileName = "f", ContentType = "text/plain",
            CreatedById = OtherUserId
        };
        _documentRepository.Setup(r => r.GetByIdAsync(docId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>())).ReturnsAsync(existing);

        Assert.That(async () => await _service.UpdateDocumentAsync(docId, MakeUpsert(null, null),
                CancellationToken.None),
            Throws.TypeOf<ForbiddenException>());
    }

    [Test]
    public void UpdateDocumentAsync_Throws_Forbidden_WhenNonStaffOwnerTriesToMakePrivate()
    {
        _authorizationService.Setup(a => a.GetCurrentUserType()).Returns(UserType.Student);
        var docId = Guid.NewGuid();
        var existing = new Document
        {
            Id = docId, Title = "x", DirectoryId = Guid.NewGuid(),
            StorageKey = "k", FileName = "f", ContentType = "text/plain",
            CreatedById = CurrentUserId, IsPrivate = false
        };
        _documentRepository.Setup(r => r.GetByIdAsync(docId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>())).ReturnsAsync(existing);

        var model = MakeUpsert(null, null, isPrivate: true);

        Assert.That(async () => await _service.UpdateDocumentAsync(docId, model, CancellationToken.None),
            Throws.TypeOf<ForbiddenException>());
    }

    // ─── DeleteDocumentAsync ─────────────────────────────────────────────────

    [Test]
    public async Task DeleteDocumentAsync_SoftDelete_DoesNotTouchStorage()
    {
        var docId = Guid.NewGuid();
        var existing = new Document
        {
            Id = docId, StorageKey = "k", FileName = "f", ContentType = "text/plain",
            CreatedById = CurrentUserId
        };
        _documentRepository.Setup(r => r.GetByIdAsync(docId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>())).ReturnsAsync(existing);
        _documentRepository.Setup(r => r.DeleteAsync(docId, It.IsAny<CancellationToken>(), true, It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(true);

        await _service.DeleteDocumentAsync(docId, CancellationToken.None, softDelete: true);

        _storageProvider.Verify(s => s.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task DeleteDocumentAsync_HardDelete_DeletesDbThenStorage()
    {
        var docId = Guid.NewGuid();
        var existing = new Document
        {
            Id = docId, StorageKey = "k", FileName = "f", ContentType = "text/plain",
            CreatedById = CurrentUserId
        };
        var sequence = new MockSequence();
        _documentRepository.Setup(r => r.GetByIdAsync(docId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>())).ReturnsAsync(existing);
        // Sequence: DB delete must happen BEFORE storage delete so a DB failure doesn't orphan
        // the row pointing at a missing blob.
        _documentRepository.InSequence(sequence)
            .Setup(r => r.DeleteAsync(docId, It.IsAny<CancellationToken>(), false, It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(true);
        _storageProvider.InSequence(sequence)
            .Setup(s => s.DeleteFileAsync("k", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.DeleteDocumentAsync(docId, CancellationToken.None, softDelete: false);

        _documentRepository.Verify(r => r.DeleteAsync(docId, It.IsAny<CancellationToken>(), false, It.IsAny<IDbTransaction?>()), Times.Once);
        _storageProvider.Verify(s => s.DeleteFileAsync("k", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteDocumentAsync_HardDelete_StorageFailure_LogsButDoesNotThrow()
    {
        var docId = Guid.NewGuid();
        var existing = new Document
        {
            Id = docId, StorageKey = "k", FileName = "f", ContentType = "text/plain",
            CreatedById = CurrentUserId
        };
        _documentRepository.Setup(r => r.GetByIdAsync(docId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>())).ReturnsAsync(existing);
        _documentRepository.Setup(r => r.DeleteAsync(docId, It.IsAny<CancellationToken>(), false, It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(true);
        _storageProvider.Setup(s => s.DeleteFileAsync("k", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("blob gone"));

        // The DB row is already gone — we don't roll back. Storage-cleanup failure is logged
        // and the operation reports success (orphan blob, not a hard failure).
        Assert.DoesNotThrowAsync(async () =>
            await _service.DeleteDocumentAsync(docId, CancellationToken.None, softDelete: false));
    }

    [Test]
    public void DeleteDocumentAsync_Throws_Forbidden_WhenNonStaffDeletesPrivate()
    {
        _authorizationService.Setup(a => a.GetCurrentUserType()).Returns(UserType.Student);
        var docId = Guid.NewGuid();
        var existing = new Document
        {
            Id = docId, StorageKey = "k", FileName = "f", ContentType = "text/plain",
            CreatedById = CurrentUserId, IsPrivate = true
        };
        _documentRepository.Setup(r => r.GetByIdAsync(docId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>())).ReturnsAsync(existing);

        Assert.That(async () => await _service.DeleteDocumentAsync(docId, CancellationToken.None),
            Throws.TypeOf<ForbiddenException>());
    }

    [Test]
    public void DeleteDocumentAsync_Throws_NotFound_WhenMissing()
    {
        var docId = Guid.NewGuid();
        _documentRepository.Setup(r => r.GetByIdAsync(docId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync((Document?)null);

        Assert.That(async () => await _service.DeleteDocumentAsync(docId, CancellationToken.None),
            Throws.TypeOf<NotFoundException>());
    }
}
