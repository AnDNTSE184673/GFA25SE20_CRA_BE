using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.CustomFunctions.SupabaseFileUploader
{
    public class UploadFile (IConfiguration config)
    {
        public async Task<byte[]> DownloadImageAsync(string prefix, string username, string subject, string folderName)
        {
            try
            {
                var url = config["Supabase:Url"];
                var key = config["Supabase:PrivateKey"];

                /*var options = new Supabase.SupabaseOptions
                {
                    AutoConnectRealtime = true
                };*/

                var supabase = new Supabase.Client(url, key, null);
                await supabase.InitializeAsync();
                string fileName = $"{prefix}_{username}_{subject}.png";
                var imagePath = Path.Combine(folderName, fileName);

                //upload file to bucket via supabase url and secret key (dont need s3 key)
                var bytes = await supabase.Storage
                    .From("TestBucket").DownloadPublicFile(imagePath);

                return bytes;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            try
            {
                var url = config["Supabase:Url"];
                var key = config["Supabase:PrivateKey"];

                /*var options = new Supabase.SupabaseOptions
                {
                    AutoConnectRealtime = true
                };*/

                var supabase = new Supabase.Client(url, key, null);
                await supabase.InitializeAsync();

                var imagePath = Path.Combine("Image", file.FileName);

                //upload file to bucket via supabase url and secret key (dont need s3 key)
                await supabase.Storage
                  .From("TestBucket")
                  .Upload(await file.GetBytesAsync(), imagePath, new Supabase.Storage.FileOptions { CacheControl = "3600", Upsert = false });

                //make sure the bucket you're connecting to is a public bucket (dropdown and make public)
                //return public bucket link
                var imageUrl = supabase.Storage
                                            .From("TestBucket")
                                            .GetPublicUrl(imagePath);
                return imageUrl;
            }
            catch (Exception ex)
            {
                return "\nDetail: " + ex.Message;
            }
        }

        public async Task<string> UploadImageMemStreamAsync(IFormFile file, string folderName)
        {
            try
            {
                var url = config["Supabase:Url"];
                var key = config["Supabase:PrivateKey"];

                /*var options = new Supabase.SupabaseOptions
                {
                    AutoConnectRealtime = true
                };*/

                var supabase = new Supabase.Client(url, key, null);
                await supabase.InitializeAsync();

                var imagePath = Path.Combine(folderName, file.FileName);

                //upload file to bucket via supabase url and secret key (dont need s3 key)
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    await supabase.Storage
                        .From("TestBucket")
                        .Upload(memoryStream.ToArray(), imagePath, new Supabase.Storage.FileOptions { CacheControl = "3600", Upsert = false });
                }

                //make sure the bucket you're connecting to is a public bucket (dropdown and make public)
                //return public bucket link
                var imageUrl = supabase.Storage
                                            .From("TestBucket")
                                            .GetPublicUrl(imagePath);
                return imageUrl;
            }
            catch (Exception ex)
            {
                return "\nDetail: " + ex.Message;
            }
        }
    }
}
