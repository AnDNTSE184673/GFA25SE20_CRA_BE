using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Extension.SupabaseFileUploader
{
    //NOTE: This class is a pair with another helper class named MimeTypeHelper

    public class FileValidationHelper
    {
        public static FileValidationResult Validate(IFormFile file, FileValidationOptions options)
        {
            if (file == null || file.Length == 0)
                return FileValidationResult.Fail("File is missing or empty.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!options.AllowedExtensions.Contains(ext))
                return FileValidationResult.Fail($"Unsupported file extension: {ext}");

            var mime = MimeTypeHelper.GetMimeType(ext);

            if (!options.AllowedMimeTypes.Contains(mime))
                return FileValidationResult.Fail($"Unsupported MIME type: {mime}");

            if (file.Length > options.MaxFileSizeBytes)
                return FileValidationResult.Fail("File exceeds maximum allowed size.");

            if (!MimeTypeHelper.IsValidFile(file))
                return FileValidationResult.Fail("File signature does not match extension.");

            return FileValidationResult.Success();
        }

    }

    public static class FileValidationPolicyFactory
    {
        /// <summary>
        /// Pass in collection of extension as string (".jpg", ".png", etc) and int32 file size limit as Megabytes (MB)
        /// </summary>
        public static FileValidationOptions CreateFromExtensions(
            IEnumerable<string> extensions,
            long maxFileSizeInMBs)
        {
            var normalizedExts = extensions
                .Select(e => e.ToLowerInvariant())
                .ToHashSet();

            var mimeTypes = normalizedExts
                .Select(MimeTypeHelper.GetMimeType)
                .Where(m => m != "application/octet-stream")
                .ToHashSet();

            return new FileValidationOptions
            {
                AllowedExtensions = normalizedExts,
                AllowedMimeTypes = mimeTypes,
                MaxFileSizeBytes = maxFileSizeInMBs * 1024 * 1024
            };
        }
    }

    /// <summary>
    /// Ext and mime types list is a HashSet and file size limit is in Bytes (B), default 50MB
    /// </summary>
    public sealed class FileValidationOptions
    {
        public ISet<string> AllowedExtensions { get; init; } = new HashSet<string>();
        public ISet<string> AllowedMimeTypes { get; init; } = new HashSet<string>();
        public long MaxFileSizeBytes { get; init; } = 50 * 1024 * 1024; // default 50MB
    }

    public sealed class FileValidationResult
    {
        public bool IsValid { get; }
        public string? Error { get; }

        private FileValidationResult(bool isValid, string? error)
        {
            IsValid = isValid;
            Error = error;
        }

        public static FileValidationResult Success() => new(true, null);
        public static FileValidationResult Fail(string error) => new(false, error);
    }
}
