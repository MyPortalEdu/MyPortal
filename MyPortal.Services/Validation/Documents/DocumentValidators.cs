using FluentValidation;
using Microsoft.Extensions.Options;
using MyPortal.Common.Constants;
using MyPortal.Common.Options;
using MyPortal.Contracts.Models.Documents;

namespace MyPortal.Services.Validation.Documents;

public class DocumentValidators
{
    public class DocumentUpsertRequestValidator : AbstractValidator<DocumentUpsertRequest>
    {
        public DocumentUpsertRequestValidator(IOptions<FileStorageOptions> fileStorageOptions)
        {
            RuleFor(x => x.TypeId)
                .NotEmpty().WithMessage("TypeId is required.");

            RuleFor(x => x.DirectoryId)
                .NotEmpty().WithMessage("DirectoryId is required.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(256).WithMessage("Title must not exceed 256 characters.")
                .Matches(RegularExpressions.AllowedDocumentNameChars)
                .WithMessage("Title must not contain illegal characters.");

            RuleFor(x => x.Description)
                .MaximumLength(256)
                .When(x => !string.IsNullOrWhiteSpace(x.Description))
                .WithMessage("Description must not exceed 256 characters.");
            
            // File-related rules: only enforced when a file is actually supplied
            When(x => x.Content != null, () =>
            {
                RuleFor(x => x.FileName)
                    .NotEmpty().WithMessage("FileName is required.")
                    .MaximumLength(256).WithMessage("FileName must not exceed 256 characters.");

                RuleFor(x => x.ContentType)
                    .NotEmpty().WithMessage("ContentType is required.")
                    .MaximumLength(256).WithMessage("ContentType must not exceed 256 characters.");

                RuleFor(x => x.SizeBytes)
                    .GreaterThan(0).WithMessage("Size must be greater than zero.")
                    .LessThan(fileStorageOptions.Value.MaxFileSizeBytes)
                    .WithMessage($"File cannot exceed {fileStorageOptions.Value.MaxFileSizeBytes / DocumentLimits.BytesPerMegabyte}MB in size.");
            });
        }
    }
}