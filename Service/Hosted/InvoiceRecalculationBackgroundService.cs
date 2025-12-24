using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Data.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Hosted
{
    public class InvoiceRecalculationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<InvoiceRecalculationBackgroundService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(3);
        private const int MaxDeadlockRetries = 3;

        public InvoiceRecalculationBackgroundService(IServiceProvider provider, ILogger<InvoiceRecalculationBackgroundService> logger)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("InvoiceRecalculationBackgroundService started. Interval: {Interval}", _interval);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RecalculateOnceAsync(stoppingToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // expected on shutdown
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while recalculating invoice totals.");
                }

                await Task.Delay(_interval, stoppingToken).ConfigureAwait(false);
            }

            _logger.LogInformation("InvoiceRecalculationBackgroundService stopping.");
        }

        private async Task RecalculateOnceAsync(CancellationToken ct)
        {
            using var scope = _provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CRA_DbContext>();

            _logger.LogDebug("Starting invoice totals scan.");

            // Read invoices and items with no tracking to avoid holding EF change tracking locks.
            var invoices = await db.Invoices
                .AsNoTracking()
                .Include(i => i.InvoiceItems)
                .ToListAsync(ct)
                .ConfigureAwait(false);

            if (invoices == null || invoices.Count == 0)
            {
                _logger.LogDebug("No invoices found to recalculate.");
                return;
            }

            _logger.LogInformation("Recalculating GrandTotal for {Count} invoices...", invoices.Count);

            foreach (var invoice in invoices)
            {
                if (ct.IsCancellationRequested) break;

                try
                {
                    var computedGrandTotal = invoice.InvoiceItems?.Sum(i => i.Total) ?? 0m;

                    if (invoice.GrandTotal == computedGrandTotal)
                    {
                        continue; // no change
                    }

                    var attempts = 0;
                    var success = false;
                    while (!success && attempts < MaxDeadlockRetries && !ct.IsCancellationRequested)
                    {
                        attempts++;
                        try
                        {
                            // Update only the GrandTotal column and only when it differs - single SQL statement to minimize locks.
                            var rows = await db.Database.ExecuteSqlInterpolatedAsync(
                                $"UPDATE \"Invoices\" SET \"GrandTotal\" = {computedGrandTotal}, \"CreateDate\" = \"CreateDate\" WHERE \"Id\" = {invoice.Id} AND \"GrandTotal\" <> {computedGrandTotal}",
                                ct).ConfigureAwait(false);

                            if (rows > 0)
                            {
                                _logger.LogInformation("Invoice {InvoiceId} GrandTotal updated from {Old} to {New}", invoice.Id, invoice.GrandTotal, computedGrandTotal);
                            }
                            else
                            {
                                // Either another process already updated it, or value is now equal — treat as success.
                                _logger.LogDebug("Invoice {InvoiceId} GrandTotal already up-to-date or changed concurrently.", invoice.Id);
                            }

                            success = true;
                        }
                        catch (Npgsql.PostgresException pgEx) when (pgEx.SqlState == "40P01")
                        {
                            // deadlock detected - retry with jitter/backoff
                            var delay = TimeSpan.FromMilliseconds(100 * attempts) + TimeSpan.FromMilliseconds(new Random().Next(0, 100));
                            _logger.LogWarning(pgEx, "Deadlock updating invoice {InvoiceId}, attempt {Attempt}. Retrying after {Delay}ms", invoice.Id, attempts, delay.TotalMilliseconds);
                            await Task.Delay(delay, ct).ConfigureAwait(false);
                        }
                        catch (DbUpdateException dbEx) when (dbEx.InnerException is Npgsql.PostgresException pg && pg.SqlState == "40P01")
                        {
                            // Another way deadlock may surface from EF - same handling
                            var delay = TimeSpan.FromMilliseconds(100 * attempts) + TimeSpan.FromMilliseconds(new Random().Next(0, 100));
                            _logger.LogWarning(dbEx, "Deadlock (DbUpdate) updating invoice {InvoiceId}, attempt {Attempt}. Retrying after {Delay}ms", invoice.Id, attempts, delay.TotalMilliseconds);
                            await Task.Delay(delay, ct).ConfigureAwait(false);
                        }
                        catch (Exception innerEx)
                        {
                            _logger.LogWarning(innerEx, "Failed to update GrandTotal for invoice {InvoiceId}. Skipping.", invoice.Id);
                            success = true; // stop retrying on unknown error to avoid infinite loop; move to next invoice
                        }
                    }

                    if (!success)
                    {
                        _logger.LogError("Failed to update invoice {InvoiceId} after {Retries} attempts due to repeated deadlocks.", invoice.Id, MaxDeadlockRetries);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed while processing invoice {InvoiceId}.", invoice?.Id);
                }
            }

            _logger.LogInformation("Invoice recalculation run finished.");
        }
    }
}
