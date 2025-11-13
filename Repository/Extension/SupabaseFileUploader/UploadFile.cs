using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Serilog;
using Supabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.CustomFunctions.SupabaseFileUploader
{
    //public class UploadFile (IConfiguration config)
    public class UploadFile
    {
        private readonly Supabase.Client _supabase;

        public UploadFile(Client supabase)
        {
            _supabase = supabase;
        }

        public async Task<byte[]> DownloadImageAsync(string prefix, string username, string subject, string folderName, string targetBucket)
        {
            try
            {
                /*var url = config["Supabase:Url"];
                var key = config["Supabase:PrivateKey"];

                var options = new Supabase.SupabaseOptions
                {
                    AutoConnectRealtime = true
                };

                var supabase = new Supabase.Client(url, key, null);*/
                await _supabase.InitializeAsync();

                string fileName = $"{prefix}_{username}_{subject}.png";
                var imagePath = Path.Combine(folderName, fileName);

                //upload file to bucket via supabase url and secret key (dont need s3 key)
                var bytes = await _supabase.Storage
                    .From(targetBucket).DownloadPublicFile(imagePath);

                return bytes;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file, string fileName, string imagePath, string targetBucket, int signedExpirationTimeSec)
        {
            var allowedExtensions = new[]
            {
                ".jpg", ".jpeg", ".png", ".gif", ".webp", // image types
                ".pdf", ".doc", ".docx"                   // document types
            };
            try
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    throw new InvalidOperationException($"File type '{extension}' is not supported.");
                }

                await _supabase.InitializeAsync();

                //upload file to bucket via supabase url and secret key (dont need s3 key)
                //await using var fileStream = file.OpenReadStream()

                var bytes = await file.GetBytesAsync();

                // Ensure ContentType is set manually
                var mimeType = extension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    ".webp" => "image/webp",
                    ".pdf" => "application/pdf",
                    ".doc" => "application/msword",
                    ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    _ => "application/octet-stream"
                };

                await _supabase.Storage.From(targetBucket)
                  .Upload(bytes,
                            imagePath,
                            new Supabase.Storage.FileOptions
                            {
                                CacheControl = "3600",
                                ContentType = mimeType,
                                Upsert = true
                            });

                //make sure the bucket you're connecting to is a public bucket (dropdown and make public)
                //return public bucket link

                return await CreateSignedUrlAsync(targetBucket, imagePath, signedExpirationTimeSec);
            }
            catch (Exception ex)
            {
                return "\nDetail: " + ex.Message;
            }
        }

        public async Task<string> UploadImageMemStreamAsync(IFormFile file, Guid carId, Guid ownerId, string targetBucket)
        {
            try
            {
                /*var url = config["Supabase:Url"];
                var key = config["Supabase:PrivateKey"];

                var options = new Supabase.SupabaseOptions
                {
                    AutoConnectRealtime = true
                };*/

                //var supabase = new Supabase.Client(url, key, null);
                await _supabase.InitializeAsync();

                string uploadDate = DateTime.Now.ToString("ddMMyyyy");

                var fileName = $"{carId}_{uploadDate}";
                //var imagePath = Path.Combine(ownerId.ToString(), fileName);
                var imagePath = $"{ownerId.ToString()}/{fileName}";

                //await using var fileStream = file.OpenReadStream()

                var bytes = await file.GetBytesAsync();

                // Ensure ContentType is set manually
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var mimeType = extension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    ".webp" => "image/webp",
                    _ => "application/octet-stream"
                };

                //upload file to bucket via supabase url and secret key (dont need s3 key)
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    await _supabase.Storage
                        .From(targetBucket)
                        .Upload(memoryStream.ToArray(), imagePath, new Supabase.Storage.FileOptions { CacheControl = "3600", ContentType = mimeType, Upsert = false });
                }

                //make sure the bucket you're connecting to is a public bucket (dropdown and make public)
                //return public bucket link
                return await GetPublicUrlAsync(targetBucket, imagePath);
            }
            catch (Exception ex)
            {
                return "\nDetail: " + ex.Message;
            }
        }

        public async Task<string> GetPublicUrlAsync(string bucketName, string filePath)
        {
            var bucket = _supabase.Storage.From(bucketName);
            return bucket.GetPublicUrl(filePath);
        }

        public async Task<string> CreateSignedUrlAsync(string bucketName, string filePath, int expirySeconds)
        {
            var bucket = _supabase.Storage.From(bucketName);
            var result = await bucket.CreateSignedUrl(filePath, expirySeconds);
            Log.Information("Signed URL generated: {Url}", result);
            if (result.EndsWith("?"))
                result = result[..^1]; //remove the last character
            return result;
        }
    }
}