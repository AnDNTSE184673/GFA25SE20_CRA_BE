using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Repository.Base;
using Repository.Constant;
using Repository.Data.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Hosted
{
    public class InvoiceStatusBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<InvoiceStatusBackgroundService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(3);

        public InvoiceStatusBackgroundService(IServiceProvider provider, ILogger<InvoiceStatusBackgroundService> logger)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("InvoiceStatusBackgroundService started. Interval: {Interval}", _interval);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunOnceAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while running invoice status reconciliation.");
                }

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("InvoiceStatusBackgroundService stopping.");
        }

        private async Task RunOnceAsync(CancellationToken ct)
        {
            using var scope = _provider.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

            try
            {
                // Start transaction for the entire reconciliation run
                await uow.BeginTransactionAsync();

                var bookings = await uow._bookingRepo.GetAllBookings();
                if (bookings == null || bookings.Count == 0)
                {
                    _logger.LogInformation("No bookings found to reconcile.");
                    await uow.CommitTransactionAsync();
                    return;
                }

                _logger.LogInformation("Reconciling {Count} bookings to update invoices...", bookings.Count);

                foreach (var booking in bookings)
                {
                    try
                    {
                        if (booking == null) continue;
                        if (booking.InvoiceId == Guid.Empty) continue;

                        var invoice = await uow._invoiceRepo.GetByIdAsync(booking.InvoiceId);
                        if (invoice == null) continue;

                        var bookingStatus = (booking.Status ?? string.Empty).Trim().ToLowerInvariant();

                        // CANCELLED
                        if (ConstantEnum.Statuses.CANCELLED.ToLower().Equals(bookingStatus))
                        {
                            invoice.Status = ConstantEnum.Status.Cancelled.ToString();
                            uow._invoiceRepo.Update(invoice);
                        }

                        // CONFIRMED
                        else if (ConstantEnum.Statuses.CONFIRMED.ToLower().Equals(bookingStatus))
                        {
                            invoice.Status = ConstantEnum.Status.Completed.ToString();
                            uow._invoiceRepo.Update(invoice);
                        }

                        // COMPLETED
                        else if (ConstantEnum.Statuses.COMPLETED.ToLower().Equals(bookingStatus))
                        {
                            invoice.Status = ConstantEnum.Status.Completed.ToString();
                            uow._invoiceRepo.Update(invoice);
                        }

                        // Save per booking to reduce transaction size (optional)
                        await uow.SaveChangesAsync();
                    }
                    catch (Exception innerEx)
                    {
                        _logger.LogWarning(innerEx, "Failed to reconcile booking {BookingId}", booking?.Id);
                        // continue with next booking
                    }
                }

                await uow.CommitTransactionAsync();
                _logger.LogInformation("Invoice reconciliation run finished successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reconcilation failed; rolling back.");
                try { await uow.RollbackTransactionAsync(); } catch { /* ignore rollback errors */ }
                throw;
            }
        }
    }
}
