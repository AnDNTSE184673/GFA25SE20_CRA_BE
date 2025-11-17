using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Extension.SupabaseFileUploader
{
    public static class MimeTypeHelper
    {
        // -----------------------------
        // MIME TYPE LOOKUP TABLE
        // -----------------------------
        private static readonly Dictionary<string, string> _mappings =
            new(StringComparer.OrdinalIgnoreCase)
        {
        // Images
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png", "image/png" },
        { ".gif", "image/gif" },
        { ".webp", "image/webp" },
        { ".bmp", "image/bmp" },
        { ".tif", "image/tiff" },
        { ".tiff", "image/tiff" },
        { ".svg", "image/svg+xml" },
        { ".ico", "image/x-icon" },
        { ".heic", "image/heic" },
        { ".heif", "image/heif" },

        // Videos
        { ".mp4", "video/mp4" },
        { ".webm", "video/webm" },
        { ".mov", "video/quicktime" },
        { ".mkv", "video/x-matroska" },
        { ".avi", "video/x-msvideo" },
        { ".wmv", "video/x-ms-wmv" },
        { ".flv", "video/x-flv" },
        { ".m4v", "video/x-m4v" },

        // Audio
        { ".mp3", "audio/mpeg" },
        { ".wav", "audio/wav" },
        { ".ogg", "audio/ogg" },
        { ".m4a", "audio/mp4" },
        { ".aac", "audio/aac" },
        { ".flac", "audio/flac" },
        { ".wma", "audio/x-ms-wma" },

        // Documents
        { ".pdf", "application/pdf" },
        { ".doc", "application/msword" },
        { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { ".xls", "application/vnd.ms-excel" },
        { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        { ".ppt", "application/vnd.ms-powerpoint" },
        { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
        { ".txt", "text/plain" },
        { ".csv", "text/csv" },
        { ".rtf", "application/rtf" },
        { ".odt", "application/vnd.oasis.opendocument.text" },
        { ".ods", "application/vnd.oasis.opendocument.spreadsheet" },
        { ".odp", "application/vnd.oasis.opendocument.presentation" },

        // Archives
        { ".zip", "application/zip" },
        { ".rar", "application/vnd.rar" },
        { ".7z", "application/x-7z-compressed" },
        { ".tar", "application/x-tar" },
        { ".gz", "application/gzip" },
        };

        // -----------------------------
        // FILE SIGNATURES ("magic bytes")
        // -----------------------------
        private static readonly Dictionary<string, List<byte[]>> _signatures =
            new(StringComparer.OrdinalIgnoreCase)
        {
        { ".jpg", new() { new byte[] { 0xFF, 0xD8 } } },
        { ".jpeg", new() { new byte[] { 0xFF, 0xD8 } } },
        { ".png", new() { new byte[] { 0x89, 0x50, 0x4E, 0x47 } } },
        { ".gif", new() { new byte[] { 0x47, 0x49, 0x46, 0x38 } } },
        { ".webp", new() { new byte[] { 0x52, 0x49, 0x46, 0x46 } } }, // RIFF
        { ".mp4", new() { new byte[] { 0x66, 0x74, 0x79, 0x70 } } }, // "ftyp"
        { ".mov", new() { new byte[] { 0x6D, 0x6F, 0x6F, 0x76 } } },
        { ".avi", new() { new byte[] { 0x52, 0x49, 0x46, 0x46 } } }, // RIFF
        { ".mkv", new() { new byte[] { 0x1A, 0x45, 0xDF, 0xA3 } } }, // EBML
        { ".pdf", new() { new byte[] { 0x25, 0x50, 0x44, 0x46 } } }, // %PDF
        { ".docx", new() { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } }, // ZIP header
        { ".xlsx", new() { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } },
        { ".pptx", new() { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } },
        { ".zip", new() { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } },
        { ".rar", new() { new byte[] { 0x52, 0x61, 0x72, 0x21 } } },
        };

        // -----------------------------
        // PUBLIC METHODS
        // -----------------------------

        /// <summary>
        /// Only support media file types (documents, videos, audios, images and archives)
        /// </summary>
        public static string GetMimeType(string extension)
        {
            if (extension == null)
                return "application/octet-stream";

            extension = Normalize(extension);

            return _mappings.TryGetValue(extension, out var mime)
                ? mime
                : "application/octet-stream";
        }
        /// <summary>
        /// Validates file by comparing its header ("magic bytes") with known safe signatures.
        /// </summary>
        public static bool IsValidFile(IFormFile file)
        {
            var ext = Normalize(Path.GetExtension(file.FileName));

            // Unknown extensions = reject
            if (!_signatures.ContainsKey(ext))
                return false;

            var signatures = _signatures[ext];

            using var reader = new BinaryReader(file.OpenReadStream());

            foreach (var sig in signatures)
            {
                var header = reader.ReadBytes(sig.Length);
                file.OpenReadStream().Position = 0;

                if (header.SequenceEqual(sig))
                    return true;
            }

            return false;
        }

        private static string Normalize(string ext)
        {
            if (string.IsNullOrWhiteSpace(ext))
                return string.Empty;

            return ext.StartsWith(".") ? ext.ToLower() : "." + ext.ToLower();
        }
    }
}
