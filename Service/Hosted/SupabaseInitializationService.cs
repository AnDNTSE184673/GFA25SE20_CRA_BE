using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Repository.CustomFunctions.SupabaseFileUploader;

namespace Service.Hosted
{
    public class SupabaseInitializationService : BackgroundService
    {
        private readonly UploadFile _uploadFile;
        private readonly ILogger<SupabaseInitializationService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

        public SupabaseInitializationService(
            UploadFile uploadFile,
            ILogger<SupabaseInitializationService> logger)
        {
            _uploadFile = uploadFile;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Supabase initialization background service started.");

            // Initial warm-up on app startup
            await InitializeSupabaseSafeAsync(stoppingToken);

            // Periodic re-check
            using var timer = new PeriodicTimer(_interval);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await InitializeSupabaseSafeAsync(stoppingToken);
            }
        }

        private async Task InitializeSupabaseSafeAsync(CancellationToken token)
        {
            try
            {
                await _uploadFile.EnsureInitializedAsync();
                _logger.LogInformation("Supabase initialized / verified successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Supabase initialization failed.");
            }
        }
    }
}
